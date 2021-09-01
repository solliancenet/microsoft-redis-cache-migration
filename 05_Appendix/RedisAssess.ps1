function GetVersion($source)
{
}

function CheckModules($source, $target)
{
    
}

function  CheckAll($source, $target)
{
    CheckModules $source $target;
}

$sourceConnString = "Redis://postgres:Seattle123@localhost:5432/reg_app";
$targetConnString = "Redis://s2admin:Seattle123Seattle123@cjg-pg-single-01:5432/postgres";


$sourceConnStringA = @{};
$sourceConnStringA.Host = "localhost";
$sourceConnStringA.Port = "5432";
$sourceConnStringA.User = "postgres";
$sourceConnStringA.Password = "Seattle123";
$sourceConnStringA.DbName = "reg_app";

$targetConnStringA = @{};
$targetConnStringA.Host = "cjg-pg-single-01.postgres.instance.azure.com";
$targetConnStringA.Port = "5432";
$targetConnStringA.User = "s2admin@cjg-pg-single-01";
$targetConnStringA.Password = "Seattle123Seattle123";
$targetConnStringA.DbName = "postgres";

CheckAll $sourceConnStringA $targetConnStringA;



