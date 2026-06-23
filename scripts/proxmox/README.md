# Fathom on Proxmox VE

`fathom-lxc.sh` provisions [Fathom](../../README.md) (a Kavita fork) on Proxmox VE.

It works in two modes:

1. **Proxmox host mode** (default) — run it on a PVE host as `root`. It creates an
   unprivileged Debian 12 LXC, installs Fathom inside it, sets up a `systemd`
   service, and prints the URL.
2. **Direct mode** — run it inside an existing Debian/Ubuntu container or VM with
   `INSTALL_HERE=1` to install Fathom onto that machine.

By default Fathom is **built from source** (this fork has no published release
tarball yet). Once you publish a release, set `FATHOM_TARBALL_URL` to the
`kavita-linux-x64.tar.gz` asset for a fast, lightweight install with no build
toolchain.

## Quick start

On a Proxmox VE host shell (as root):

```bash
bash scripts/proxmox/fathom-lxc.sh
```

Then open `http://<container-ip>:5000` and create your admin account.

## Common overrides

```bash
# Custom container resources / placement
CTID=131 HOSTNAME=fathom CORES=4 RAM_MB=4096 DISK_GB=16 \
BRIDGE=vmbr0 STORAGE=local-lvm bash scripts/proxmox/fathom-lxc.sh

# Static IP
NET=192.168.1.50/24 GW=192.168.1.1 bash scripts/proxmox/fathom-lxc.sh

# Fast path: install a prebuilt release tarball (no build)
FATHOM_TARBALL_URL=https://github.com/<you>/Fathom/releases/download/v1.0.0/kavita-linux-x64.tar.gz \
  bash scripts/proxmox/fathom-lxc.sh

# Install directly into the current Debian/Ubuntu machine
INSTALL_HERE=1 bash scripts/proxmox/fathom-lxc.sh
```

## Variables

| Variable | Default | Purpose |
| --- | --- | --- |
| `CTID` | next free id | LXC container id |
| `HOSTNAME` | `fathom` | Container hostname |
| `CORES` | `2` | vCPUs |
| `RAM_MB` | `4096` build / `2048` tarball | Memory (MB) |
| `DISK_GB` | `12` build / `8` tarball | Root disk (GB) |
| `BRIDGE` | `vmbr0` | Network bridge |
| `NET` | `dhcp` | `dhcp` or a static CIDR (e.g. `192.168.1.50/24`) |
| `GW` | — | Gateway (required for a static `NET`) |
| `STORAGE` | auto / `local-lvm` | Storage for the container rootfs |
| `TEMPLATE_STORAGE` | auto / `local` | Storage holding the LXC template |
| `UNPRIVILEGED` | `1` | Create an unprivileged container |
| `PASSWORD` | random | Container root password (printed at the end) |
| `PORT` | `5000` | Fathom web UI port |
| `REPO_URL` | `https://github.com/risacaph/Kavita` | Source repo (build mode) |
| `BRANCH` | `develop` | Branch/tag to build |
| `FATHOM_TARBALL_URL` | — | Prebuilt tarball URL (skips building) |
| `SLIM` | `yes` | Remove the build toolchain after a source build |
| `INSTALL_HERE` | `0` | Install on the current machine instead of creating an LXC |

## What gets installed

- App directory: `/opt/fathom` (binary `Fathom`; data + config under `/opt/fathom/config`)
- Runtime user: `fathom` (system account, no shell)
- Service: `fathom.service` — `systemctl status fathom`, logs via `journalctl -u fathom -e`
- Runtime dependencies: `libicu-dev`, `libgdiplus`, `tzdata`, `curl`
  (the same set used by the Fathom Docker image)

## Updating

- **Tarball installs:** stop the service, replace everything in `/opt/fathom`
  except `config/`, then start it again:
  ```bash
  systemctl stop fathom
  # extract the new kavita-linux-x64.tar.gz over /opt/fathom (keep config/)
  chown -R fathom:fathom /opt/fathom
  systemctl start fathom
  ```
- **Source installs:** re-run the script in `INSTALL_HERE=1` mode inside the
  container; the existing `config/` is preserved.

## Notes

- Build-from-source pulls the .NET 10 SDK and Node.js 22 into the container,
  compiles the Angular UI and publishes a self-contained server, then (with
  `SLIM=yes`) removes the toolchain. Give it a few minutes and enough RAM.
- A source build clones `REPO_URL` over HTTPS, so that repo/branch must be
  reachable (public, or otherwise accessible from the container).
