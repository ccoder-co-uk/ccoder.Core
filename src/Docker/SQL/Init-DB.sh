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

echo "Creating empty databases"

/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "test123!!" -C -Q "CREATE DATABASE dev-Core"

/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "test123!!" -C -Q "CREATE DATABASE dev-SSO"

echo "Spinning up Hosted Services"

ls /tmp/hostedServices

cd /tmp/hostedServices

# Allow executing the program

chmod a+x /tmp/hostedServices/HostedServices

# Execute hosted services

/tmp/hostedServices/HostedServices &

sleep 5

echo "Migrating Database"

wget --method=POST --no-check-certificate http://localhost:5000/Scheduler/Migrate

pkill sqlservr

rm -rf /home/app

rm /tmp/Init-DB.sh
rm /tmp/hostedServices/appsettings.json