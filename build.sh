#! /bin/bash
set -e

outputFolder='_output'

CheckRequirements()
{
    if ! command -v npm &> /dev/null
    then
        echo "Warning!!! npm not found, it is required for building Fathom!"
    fi
    if ! command -v dotnet &> /dev/null
    then
        echo "Warning!!! dotnet not found, it is required for building Fathom!"
    fi
}

ProgressStart()
{
    echo "Start '$1'"
}

ProgressEnd()
{
    echo "Finish '$1'"
}


Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder

    slnFile=Fathom.sln

    dotnet clean $slnFile -c Release

    if [[ -z "$RID" ]];
    then
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform="Any CPU"
    else
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform="Any CPU" -p:RuntimeIdentifiers=$RID
    fi

    ProgressEnd 'Build'
}

BuildUI()
{
    ProgressStart 'Building UI'
    echo 'Removing old wwwroot'
    rm -rf Fathom.Server/wwwroot/*
    cd UI/Web/ || exit
    echo 'Installing web dependencies'
    npm ci
    echo 'Building UI'
    npm run prod
    echo 'Copying back to Fathom wwwroot'
    mkdir -p ../../Fathom.Server/wwwroot
    cp -R dist/browser/* ../../Fathom.Server/wwwroot
    cd ../../ || exit
    ProgressEnd 'Building UI'
}

Package()
{
    local runtime="$1"
    local lOutputFolder=../_output/"$runtime"/Fathom

    ProgressStart "Creating $runtime Package"

    # TODO: Use no-restore? Because Build should have already done it for us
    echo "Building"
    cd Fathom.Server
    echo dotnet publish -c Release --self-contained --runtime $runtime -o "$lOutputFolder"
    dotnet publish -c Release --self-contained --runtime $runtime -o "$lOutputFolder"

    echo "Recopying wwwroot due to bug"
    cp -R ./wwwroot/* $lOutputFolder/wwwroot

    echo "Removing EF Core design-time folders"
    rm -rf "$lOutputFolder"/BuildHost-net472
    rm -rf "$lOutputFolder"/BuildHost-netcore

    echo "Removing cache-long from config"
    rm -rf "$lOutputFolder"/config/cache-long

    echo "Copying Install information"
    cp ../INSTALL.txt "$lOutputFolder"/README.txt

    echo "Copying LICENSE"
    cp ../LICENSE "$lOutputFolder"/LICENSE.txt

    echo "Renaming Fathom.Server -> Fathom"
    if [ $runtime == "win-x64" ] || [ $runtime == "win-x86" ]
    then
        mv "$lOutputFolder"/Fathom.Server.exe "$lOutputFolder"/Fathom.exe
    else
        mv "$lOutputFolder"/Fathom.Server "$lOutputFolder"/Fathom
    fi

    mkdir -p $lOutputFolder/config
    echo "Copying appsettings.json"
    cp config/appsettings.json $lOutputFolder/config/appsettings-init.json

    echo "Creating tar"
    cd ../$outputFolder/"$runtime"/
    tar -czvf ../fathom-$runtime.tar.gz Fathom


    ProgressEnd "Creating $runtime Package"
}


RID="$1"

CheckRequirements
BuildUI
Build

dir=$PWD

if [[ -z "$RID" ]];
then
    Package "win-x64"
    cd "$dir"
    Package "win-x86"
    cd "$dir"
    Package "linux-x64"
    cd "$dir"
    Package "linux-arm"
    cd "$dir"
    Package "linux-arm64"
    cd "$dir"
    Package "linux-musl-x64"
    cd "$dir"
    Package "osx-x64"
    cd "$dir"
    Package "osx-arm64"
    cd "$dir"
else
    Package "$RID"
    cd "$dir"
fi
