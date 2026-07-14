#!/usr/bin/env bash
#
# Fathom - Proxmox VE LXC installer
# ---------------------------------
# Fathom is a fork of Kavita (https://github.com/Kareadita/Kavita), GPL-3.0.
#
# This script has two modes:
#
#   1. Run it on a Proxmox VE host (as root) and it will create a Debian 12
#      LXC container, install Fathom inside it, and expose it on :5000.
#
#   2. Run it inside an existing Debian/Ubuntu container or VM (as root) with
#      INSTALL_HERE=1 and it will install Fathom directly on that machine.
#
# By default Fathom is BUILT FROM SOURCE (the fork has no published release
# tarball yet). Once you publish a release, point FATHOM_TARBALL_URL at the
# fathom-linux-x64.tar.gz asset for a fast, lightweight install.
#
# Usage (on a Proxmox VE host shell):
#   bash fathom-lxc.sh
#   # or with overrides:
#   CTID=131 HOSTNAME=fathom CORES=4 RAM_MB=4096 DISK_GB=16 \
#   BRIDGE=vmbr0 STORAGE=local-lvm bash fathom-lxc.sh
#
#   # fast path using a prebuilt release tarball:
#   FATHOM_TARBALL_URL=https://github.com/<you>/Fathom/releases/download/vX/fathom-linux-x64.tar.gz \
#   bash fathom-lxc.sh
#
#   # install directly into the current Debian/Ubuntu machine:
#   INSTALL_HERE=1 bash fathom-lxc.sh
#
set -Eeuo pipefail

# ---------------------------------------------------------------------------
# Configuration (override any of these via environment variables)
# ---------------------------------------------------------------------------
REPO_URL="${REPO_URL:-https://github.com/risacaph/Fathom}"   # source repo (build-from-source)
BRANCH="${BRANCH:-develop}"                                  # branch/tag to build
FATHOM_TARBALL_URL="${FATHOM_TARBALL_URL:-}"                 # set to skip building
PORT="${PORT:-5000}"                                         # web UI port
SLIM="${SLIM:-yes}"                                          # purge build toolchain afterwards

# LXC container settings (host mode only)
HOSTNAME="${HOSTNAME:-fathom}"
BRIDGE="${BRIDGE:-vmbr0}"
UNPRIVILEGED="${UNPRIVILEGED:-1}"
NET="${NET:-dhcp}"                # "dhcp" or a CIDR static IP e.g. 192.168.1.50/24
GW="${GW:-}"                      # gateway (required when NET is a static IP)
PASSWORD="${PASSWORD:-}"          # root password for the CT (random if empty)

# Resource defaults scale with the install method (build-from-source is heavier).
if [[ -n "$FATHOM_TARBALL_URL" ]]; then
    RAM_MB="${RAM_MB:-2048}"; DISK_GB="${DISK_GB:-8}"; CORES="${CORES:-2}"
else
    RAM_MB="${RAM_MB:-4096}"; DISK_GB="${DISK_GB:-12}"; CORES="${CORES:-2}"
fi

# ---------------------------------------------------------------------------
# Pretty logging
# ---------------------------------------------------------------------------
RD=$'\e[31m'; GN=$'\e[32m'; YW=$'\e[33m'; BL=$'\e[34m'; CL=$'\e[0m'
info()  { echo -e "${BL}[i]${CL} $*"; }
ok()    { echo -e "${GN}[✓]${CL} $*"; }
warn()  { echo -e "${YW}[!]${CL} $*"; }
die()   { echo -e "${RD}[x]${CL} $*" >&2; exit 1; }

trap 'die "failed at line $LINENO"' ERR

[[ $EUID -eq 0 ]] || die "Please run as root."

# ===========================================================================
# In-container / in-VM installer payload.
# This block is written verbatim and executed on the TARGET machine. It reads
# REPO_URL / BRANCH / PORT / FATHOM_TARBALL_URL / SLIM from the environment.
# ===========================================================================
read -r -d '' INSTALL_PAYLOAD <<'PAYLOAD' || true
#!/usr/bin/env bash
set -Eeuo pipefail
export DEBIAN_FRONTEND=noninteractive

REPO_URL="${REPO_URL:-https://github.com/risacaph/Fathom}"
BRANCH="${BRANCH:-develop}"
PORT="${PORT:-5000}"
FATHOM_TARBALL_URL="${FATHOM_TARBALL_URL:-}"
SLIM="${SLIM:-yes}"
APP_DIR="/opt/fathom"

say() { echo -e "\e[1;34m[fathom]\e[0m $*"; }

say "Installing runtime dependencies..."
apt-get update -qq
# Mirrors the dependencies used by the official Fathom/Kavita Docker image.
apt-get install -y --no-install-recommends \
    curl ca-certificates libicu-dev libgdiplus tzdata >/dev/null

mkdir -p "$APP_DIR"

if [[ -n "$FATHOM_TARBALL_URL" ]]; then
    say "Fetching prebuilt tarball: $FATHOM_TARBALL_URL"
    curl -fSL "$FATHOM_TARBALL_URL" -o /tmp/fathom.tar.gz
    tmp="$(mktemp -d)"
    tar -xzf /tmp/fathom.tar.gz -C "$tmp"
    # The release tarball contains a single top-level app directory.
    src="$(find "$tmp" -mindepth 1 -maxdepth 1 -type d | head -n1)"
    cp -a "${src:-$tmp}"/. "$APP_DIR"/
    rm -rf "$tmp" /tmp/fathom.tar.gz
else
    say "No tarball provided - building from source ($REPO_URL @ $BRANCH)."
    apt-get install -y --no-install-recommends git build-essential >/dev/null

    say "Installing .NET 10 SDK..."
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    bash /tmp/dotnet-install.sh --channel 10.0 --install-dir /usr/local/dotnet
    export DOTNET_ROOT=/usr/local/dotnet
    export PATH="/usr/local/dotnet:$PATH"
    export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

    say "Installing Node.js 22..."
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - >/dev/null
    apt-get install -y nodejs >/dev/null

    say "Cloning source..."
    rm -rf /opt/fathom-src
    git clone --depth 1 --branch "$BRANCH" "$REPO_URL" /opt/fathom-src
    cd /opt/fathom-src

    say "Building Angular UI (this can take several minutes)..."
    ( cd UI/Web && npm ci && npm run prod )
    rm -rf Fathom.Server/wwwroot
    mkdir -p Fathom.Server/wwwroot
    cp -R UI/Web/dist/browser/* Fathom.Server/wwwroot/

    say "Publishing .NET server (self-contained linux-x64)..."
    ( cd Fathom.Server && dotnet publish -c Release --self-contained --runtime linux-x64 -o "$APP_DIR" )
    # Re-copy wwwroot (dotnet publish can miss it) and ship the first-run config template.
    mkdir -p "$APP_DIR/wwwroot"
    cp -R Fathom.Server/wwwroot/* "$APP_DIR/wwwroot/" 2>/dev/null || true
    mkdir -p "$APP_DIR/config"
    if [[ -f Fathom.Server/config/appsettings.json ]]; then
        cp Fathom.Server/config/appsettings.json "$APP_DIR/config/appsettings-init.json"
    fi
    # Trim EF Core design-time host folders that publish leaves behind.
    rm -rf "$APP_DIR"/BuildHost-net472 "$APP_DIR"/BuildHost-netcore

    cd /
    if [[ "$SLIM" == "yes" ]]; then
        say "Pruning build toolchain to slim the container..."
        rm -rf /opt/fathom-src /usr/local/dotnet ~/.nuget ~/.npm
        apt-get purge -y nodejs build-essential git >/dev/null 2>&1 || true
        apt-get autoremove -y >/dev/null 2>&1 || true
    fi
fi

# Normalise the server binary name to "Fathom" (the published apphost is "Fathom.Server";
# a prebuilt release tarball already ships it renamed to "Fathom").
if [[ -f "$APP_DIR/Fathom.Server" ]]; then
    mv -f "$APP_DIR/Fathom.Server" "$APP_DIR/Fathom"
fi
[[ -f "$APP_DIR/Fathom" ]] || { echo "ERROR: server binary not found in $APP_DIR" >&2; exit 1; }
chmod +x "$APP_DIR/Fathom"

# Config: on first run Fathom renames config/appsettings-init.json -> config/appsettings.json
# and generates a TokenKey. Make sure a template exists and carries the chosen port.
mkdir -p "$APP_DIR/config"
if [[ ! -f "$APP_DIR/config/appsettings-init.json" && ! -f "$APP_DIR/config/appsettings.json" ]]; then
    cat > "$APP_DIR/config/appsettings-init.json" <<JSON
{
  "TokenKey": "$(head -c 48 /dev/urandom | base64 | tr -d '\n')",
  "Port": $PORT,
  "IpAddresses": "",
  "BaseUrl": "/"
}
JSON
fi
if [[ "$PORT" != "5000" ]]; then
    for f in "$APP_DIR/config/appsettings-init.json" "$APP_DIR/config/appsettings.json"; do
        [[ -f "$f" ]] && sed -i -E "s/\"Port\":[[:space:]]*[0-9]+/\"Port\": $PORT/" "$f"
    done
fi

# Dedicated unprivileged service account.
if ! id fathom >/dev/null 2>&1; then
    useradd --system --home-dir "$APP_DIR" --shell /usr/sbin/nologin fathom
fi
chown -R fathom:fathom "$APP_DIR"

say "Installing systemd service..."
cat > /etc/systemd/system/fathom.service <<UNIT
[Unit]
Description=Fathom Reader
After=network-online.target
Wants=network-online.target

[Service]
Type=simple
User=fathom
Group=fathom
WorkingDirectory=$APP_DIR
ExecStart=$APP_DIR/Fathom
Restart=on-failure
RestartSec=5
Environment=DOTNET_RUNNING_IN_CONTAINER=true
Environment=TZ=UTC

[Install]
WantedBy=multi-user.target
UNIT

systemctl daemon-reload
systemctl enable --now fathom >/dev/null

say "Waiting for Fathom to report healthy..."
healthy=no
for _ in $(seq 1 60); do
    if curl -fsS "http://localhost:$PORT/api/health" >/dev/null 2>&1; then healthy=yes; break; fi
    sleep 2
done
if [[ "$healthy" == "yes" ]]; then
    say "Fathom is up (health check passed)."
else
    say "Installed, but health check has not passed yet. Inspect: journalctl -u fathom -e"
fi
PAYLOAD

# ===========================================================================
# Mode 2: install directly onto this machine.
# ===========================================================================
if [[ "${INSTALL_HERE:-0}" == "1" ]] || ! command -v pct >/dev/null 2>&1; then
    if ! command -v pct >/dev/null 2>&1 && [[ "${INSTALL_HERE:-0}" != "1" ]]; then
        warn "'pct' not found - assuming this IS the target machine (INSTALL_HERE mode)."
    fi
    command -v apt-get >/dev/null 2>&1 || die "This installer targets Debian/Ubuntu (apt-get not found)."
    info "Installing Fathom on the current machine..."
    tmpf="$(mktemp)"; printf '%s\n' "$INSTALL_PAYLOAD" > "$tmpf"
    REPO_URL="$REPO_URL" BRANCH="$BRANCH" PORT="$PORT" \
        FATHOM_TARBALL_URL="$FATHOM_TARBALL_URL" SLIM="$SLIM" \
        bash "$tmpf"
    rm -f "$tmpf"
    ip="$(hostname -I 2>/dev/null | awk '{print $1}')"
    ok "Fathom installed. Open: http://${ip:-<this-host>}:$PORT"
    exit 0
fi

# ===========================================================================
# Mode 1: provision a new LXC on this Proxmox VE host.
# ===========================================================================
command -v pveversion >/dev/null 2>&1 || die "This does not look like a Proxmox VE host."

CTID="${CTID:-$(pvesh get /cluster/nextid 2>/dev/null)}"
[[ -n "$CTID" ]] || die "Could not determine a container ID; set CTID=<n>."
if pct status "$CTID" >/dev/null 2>&1; then
    die "CTID $CTID already exists. Set CTID=<n> to a free id."
fi

# Pick storages if not provided.
if [[ -z "${STORAGE:-}" ]]; then
    STORAGE="$(pvesm status -content rootdir 2>/dev/null | awk 'NR>1{print $1; exit}')"
    STORAGE="${STORAGE:-local-lvm}"
fi
if [[ -z "${TEMPLATE_STORAGE:-}" ]]; then
    TEMPLATE_STORAGE="$(pvesm status -content vztmpl 2>/dev/null | awk 'NR>1{print $1; exit}')"
    TEMPLATE_STORAGE="${TEMPLATE_STORAGE:-local}"
fi

# Resolve / download the Debian 12 template.
info "Resolving Debian 12 LXC template..."
pveam update >/dev/null 2>&1 || true
TEMPLATE="$(pveam available --section system 2>/dev/null | awk '/debian-12-standard/{print $2}' | sort -V | tail -n1)"
[[ -n "$TEMPLATE" ]] || die "Could not find a debian-12-standard template via 'pveam available'."
if ! pveam list "$TEMPLATE_STORAGE" 2>/dev/null | grep -q "$TEMPLATE"; then
    info "Downloading template $TEMPLATE to $TEMPLATE_STORAGE..."
    pveam download "$TEMPLATE_STORAGE" "$TEMPLATE"
fi
TEMPLATE_REF="$TEMPLATE_STORAGE:vztmpl/$TEMPLATE"

# Network config.
if [[ "$NET" == "dhcp" ]]; then
    NETCFG="name=eth0,bridge=$BRIDGE,ip=dhcp"
else
    [[ -n "$GW" ]] || die "Static NET=$NET requires GW=<gateway>."
    NETCFG="name=eth0,bridge=$BRIDGE,ip=$NET,gw=$GW"
fi

# Root password.
if [[ -z "$PASSWORD" ]]; then
    PASSWORD="$(openssl rand -base64 12 2>/dev/null || head -c 12 /dev/urandom | base64)"
    GENERATED_PW=1
fi

cat <<SUMMARY
${BL}--------------------------------------------------------${CL}
 Creating Fathom LXC
   CTID ........ $CTID
   Hostname .... $HOSTNAME
   Cores/RAM ... ${CORES} vCPU / ${RAM_MB} MB
   Disk ........ ${DISK_GB} GB on $STORAGE
   Network ..... $NETCFG
   Template .... $TEMPLATE
   Method ...... $([[ -n "$FATHOM_TARBALL_URL" ]] && echo "prebuilt tarball" || echo "build from source ($REPO_URL @ $BRANCH)")
${BL}--------------------------------------------------------${CL}
SUMMARY

info "Creating container $CTID..."
pct create "$CTID" "$TEMPLATE_REF" \
    --hostname "$HOSTNAME" \
    --cores "$CORES" --memory "$RAM_MB" --swap 512 \
    --rootfs "$STORAGE:$DISK_GB" \
    --net0 "$NETCFG" \
    --unprivileged "$UNPRIVILEGED" \
    --features nesting=1 \
    --ostype debian \
    --password "$PASSWORD" \
    --onboot 1 \
    --description "Fathom Reader - http://<ip>:$PORT" >/dev/null
ok "Container created."

info "Starting container..."
pct start "$CTID"

info "Waiting for container network..."
for _ in $(seq 1 30); do
    if pct exec "$CTID" -- getent hosts github.com >/dev/null 2>&1; then break; fi
    sleep 2
done

info "Installing Fathom inside the container (this can take a while)..."
PAYLOAD_FILE="$(mktemp)"
printf '%s\n' "$INSTALL_PAYLOAD" > "$PAYLOAD_FILE"
pct push "$CTID" "$PAYLOAD_FILE" /root/fathom-install.sh --perms 0755
rm -f "$PAYLOAD_FILE"
pct exec "$CTID" -- env \
    REPO_URL="$REPO_URL" BRANCH="$BRANCH" PORT="$PORT" \
    FATHOM_TARBALL_URL="$FATHOM_TARBALL_URL" SLIM="$SLIM" \
    bash /root/fathom-install.sh

IP="$(pct exec "$CTID" -- bash -c "hostname -I 2>/dev/null | awk '{print \$1}'")"

cat <<DONE

${GN}========================================================${CL}
 Fathom is installed in LXC ${CTID} (${HOSTNAME})
   URL ............. http://${IP:-<container-ip>}:${PORT}
   CT root login ... user: root  password: ${PASSWORD}$([[ "${GENERATED_PW:-0}" == "1" ]] && echo "  (generated - save it)")
   Service ......... systemctl status fathom   (logs: journalctl -u fathom -e)
   App directory ... /opt/fathom   (data + config under /opt/fathom/config)
${GN}========================================================${CL}
 Open the URL and create your admin account to finish setup.
DONE
