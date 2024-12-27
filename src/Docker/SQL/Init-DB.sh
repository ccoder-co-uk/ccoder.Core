# Wait for SQL Server to be ready

DOTNET_VERSION="9.0"
RUNTIME_URL="https://download.visualstudio.microsoft.com/download/pr/d74fd2dd-3384-4952-924b-f5d492326e35/e91d8295d4cbe82ba3501e411d78c9b8/dotnet-sdk-9.0.101-linux-x64.tar.gz"
INSTALL_DIR="/usr/share/dotnet"

# Step 1: Download the .NET 9.0 runtime using wget
echo "Downloading .NET $DOTNET_VERSION runtime..."
wget -q $RUNTIME_URL -O dotnet-sdk.tar.gz

# Step 2: Extract the runtime to the installation directory
echo "Installing .NET SDK to $INSTALL_DIR..."
mkdir -p $INSTALL_DIR
tar -zxf dotnet-sdk.tar.gz -C $INSTALL_DIR

# Step 3: Add .NET to PATH
echo "Updating PATH..."
export PATH=$INSTALL_DIR:$PATH

echo "Spinning up SQL Server"

/opt/mssql/bin/sqlservr &

sleep 15

echo "Spinning up Hosted Services"

dotnet run /tmp/hostedServices/HostedServices.dll &

sleep 5

echo "Migrating Database"

wget --method=POST --no-check-certificate https://localhost:7100/Scheduler/Migrate

pkill sqlservr

rm -rf /home/app

rm /tmp/Init-DB.sh
rm /tmp/hostedServices/appsettings.json