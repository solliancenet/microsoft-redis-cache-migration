function GetVersion($source)
{
}

function CheckModules($source, $target)
{
    
}

function CheckUsers($source, $target)
{
    
}

function  CheckAll($source, $target)
{
    CheckModules $source $target;
}


$sourceConnStringA = @{};
$sourceConnStringA.Host = "localhost";
$sourceConnStringA.Port = "6379";
$sourceConnStringA.User = "";
$sourceConnStringA.Password = "";
$sourceConnStringA.Db = 0;

$targetConnStringA = @{};
$targetConnStringA.Host = "PREFIX-redis-ent.redis.cache.windows.net";
$targetConnStringA.Port = "6379";
$targetConnStringA.UseSSL = $true;
$targetConnStringA.User = "";
$targetConnStringA.Password = "REDIS_PWD";
$targetConnStringA.Db = 0;

CheckAll $sourceConnStringA $targetConnStringA;



