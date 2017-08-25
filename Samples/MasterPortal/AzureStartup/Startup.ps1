# Azure startup script for osFamily="3" (Windows Server 2012)

# http://technet.microsoft.com/en-us/library/cc732757.aspx

Import-Module ServerManager

# define paths and executables

$appCmdExe = Join-Path $env:windir "system32\inetsrv\appcmd.exe"

function ConfigureIIS
{
	& $appCmdExe set config "-section:applicationPools" "-applicationPoolDefaults.processModel.loadUserProfile:true"
	& $appCmdExe set config "-section:applicationPools" "-applicationPoolDefaults.processModel.idleTimeout:00:00:00"
	& $appCmdExe set config "-section:applicationPools" "-applicationPoolDefaults.recycling.periodicRestart.time:00:00:00"
}

# Type.InvokeMember
# http://msdn.microsoft.com/en-us/library/66btctbe.aspx

function SelectRelayIpList
{
	param([parameter(position=0)]$relayIpList)

	[Reflection.BindingFlags] $getFlags = "Public", "Instance", "GetProperty"

	Write-Output @{
		"GrantByDefault" = $relayIpList.GetType().InvokeMember("GrantByDefault", $getFlags, $null, $relayIpList, $null)
		"IPGrant" = $relayIpList.GetType().InvokeMember("IPGrant", $getFlags, $null, $relayIpList, $null)
	}
}

function SetRelayIpList
{
	param([parameter(position=0)]$relayIpList, [parameter(position=1)]$grantByDefault, [parameter(position=2)]$ipGrant)

	[Reflection.BindingFlags] $setFlags = "Public", "Instance", "SetProperty"

	$relayIpList.GetType().InvokeMember("GrantByDefault", $setFlags, $null, $relayIpList, $grantByDefault)
	$relayIpList.GetType().InvokeMember("IPGrant", $setFlags, $null, $relayIpList, $ipGrant)
}

function ConfigureSmtp
{
	Add-WindowsFeature SMTP-Server, windows-identity-foundation

	$smtpsvc = [adsi]"IIS://localhost/smtpsvc/1"
	$relayIpList = $smtpsvc.PSBase.Properties["RelayIpList"].Value

	SetRelayIpList $relayIpList $false "127.0.0.1, 255.255.255.255"

	$smtpsvc.Put("RelayIpList", $relayIpList)
	$smtpsvc.SetInfo()

	Set-Service SMTPSVC -StartupType Automatic
}

function Startup
{
	Write-Host "ConfigureIIS"

	ConfigureIIS

	Write-Host "ConfigureSmtp"

	ConfigureSmtp

	Write-Host "Done"
}

Startup