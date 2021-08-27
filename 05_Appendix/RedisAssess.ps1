function GetVersion($source)
{
}

function GetData($connString, $sql, $outputFile)
{
    $env:PGPASSWORD = $connString.Password;

    #$data = $sql | . $global:psqlPath\psql.exe -t $connString;
    #$data = $sql | . $global:psqlPath\psql.exe -d $connString;

    Write-Host "$global:psqlPath\psql.exe -h $($connString.Host) -p $($connString.Port) -U $($connString.User) -d $($connstring.DbName) -c `"$sql;`"";

    #Write-Host "$global:psqlPath\psql.exe -h $($connString.Host) -p $($connString.Port) -U $($connString.User) -d $($connstring.DbName) -c `"copy ($sql) TO `'$outputFile`' WITH (FORMAT CSV, HEADER)`"";

    $data = $sql | & $global:psqlPath\psql.exe -h $($connString.Host) -p $($connString.Port) -U $($connString.User) -d $($connstring.DbName) -c `"$sql`";

    #$data = $sql | & $global:psqlPath\psql.exe -h $($connString.Host) -p $($connString.Port) -U $($connString.User) -d $($connstring.DbName) -c "copy ($sql) TO `'$outputFile`' WITH (FORMAT CSV, HEADER)";

    #$data = Import-csv $outputFile;

    return $data;


}

function CheckCasts($source, $target)
{
    $sourceData = GetData $source "select castsource, casttarget, proname from pg_cast pc, pg_proc pp where pc.castfunc = pp.oid" "c:\temp\output.csv";
    $targetData = GetData $target "select castsource, casttarget, proname from pg_cast pc, pg_proc pp where pc.castfunc = pp.oid" "c:\temp\output.csv";

    $sourceHash = new-object system.collections.hashtable;
    $targetHash = new-object system.collections.hashtable;

    foreach ($field in $sourceData) 
    {
        if ($field.contains("rows)") -or $field.contains("-+-)"))
        {
            continue;
        } 
        
        $castsource, $casttarget, $proname = $field.split('|')
        $sourceHash.Add($castsource + $casttarget + $proname, $castsource + $casttarget + $proname);
    }    

    foreach ($field in $targetData) 
    { 
        if ($field.contains("rows)") -or $field.contains("-+-)"))
        {
            continue;
        } 

        $castsource, $casttarget, $proname = $field.split('|')
        $targetHash.Add($castsource + $casttarget + $proname, $castsource + $casttarget + $proname);
    }    

    foreach($key in $sourceHash.keys)
    {
        if (!$targetHash.ContainsKey($key))
        {
            write-host "Missing cast function [$key] in target [$($target.Host)]" -ForegroundColor Red;
        }
    }
}

function CheckExtensions($source, $target)
{
    $sourceData = GetData $source "SELECT * FROM pg_available_extensions" "c:\temp\output.csv";
    $targetData = GetData $target "SELECT * FROM pg_available_extensions" "c:\temp\output.csv";

    $sourceHash = new-object system.collections.hashtable;
    $targetHash = new-object system.collections.hashtable;

    foreach ($field in $sourceData) 
    {
        if ($field.contains("default_version") -or $field.contains("rows)") -or $field.contains("-+-)"))
        {
            continue;
        } 
        
        $name, $version, $blah,  $description = $field.split('|')
        $sourceHash.Add($name + $version, $name + $version);
    }    

    foreach ($field in $targetData) 
    { 
        if ($field.contains("default_version") -or $field.contains("rows)") -or $field.contains("-+-)"))
        {
            continue;
        } 

        $name, $version, $blah,  $description = $field.split('|')
        $targetHash.Add($name + $version, $name + $version);
    }    

    foreach($key in $sourceHash.keys)
    {
        if (!$targetHash.ContainsKey($key))
        {
            write-host "Missing extension [$key] in target [$($target.Host)]" -ForegroundColor Red;
        }
    }
}

function CheckExternalFunctions($source, $target)
{
    #get the list of function languages in source
    $sourceData = GetData $source "select n.nspname as function_schema,p.proname as function_name,l.lanname as function_language from pg_proc p left join pg_namespace n on p.pronamespace = n.oid left join pg_language l on p.prolang = l.oid left join pg_type t on t.oid = p.prorettype  where n.nspname not in ('pg_catalog', 'information_schema') order by function_schema, function_name;" "c:\temp\output.csv";
    

    foreach ($field in $sourceData) 
    { 
        if ($field.contains("-+-") -or $field.contains("rows)"))
        {
            continue;
        } 

        $schema, $name, $language  = $field.split('|')

        if ($language -eq "c")
        {
            write-host "External function [$key] not supported in target [$($target.Host)]" -ForegroundColor Red;
        }
    }    
}

function CheckFunctionLanguages($source, $target)
{
    #get the list of function languages in source
    $sourceData = GetData $source "select distinct l.lanname as function_language from pg_proc p left join pg_language l on p.prolang = l.oid order by function_language;" "c:\temp\output.csv";
    
    #get the list of function languages supported in target
    $targetData = GetData $target "SELECT lanname FROM pg_language" "c:\temp\output.csv";

    $sourceHash = new-object system.collections.hashtable;
    $targetHash = new-object system.collections.hashtable;

    foreach ($field in $sourceData) 
    { 
        if ($field.contains("-+-") -or $field.contains("rows)"))
        {
            continue;
        } 

        $name = $field.split('|')
        $sourceHash.Add($name.trim(), $name.trim());
    }    

    foreach ($field in $targetData) 
    { 
        if ($field.contains("-+-") -or $field.contains("rows)"))
        {
            continue;
        } 

        $name = $field.split('|')
        $targetHash.Add($name.trim(), $name.trim());
    }    

    foreach($key in $sourceHash.keys)
    {
        if (!$targetHash.ContainsKey($key))
        {
            write-host "Missing function language [$key] in target [$($target.Host)]" -ForegroundColor Red;
        }
    }
}

function CheckLanguages($source, $target)
{
    $sourceData = GetData $source "SELECT lanname FROM pg_language" "c:\temp\output.csv";
    $targetData = GetData $target "SELECT lanname FROM pg_language" "c:\temp\output.csv";

    $sourceHash = new-object system.collections.hashtable;
    $targetHash = new-object system.collections.hashtable;

    foreach ($field in $sourceData) 
    { 
        if ($field.contains("-+-") -or $field.contains("rows)"))
        {
            continue;
        } 

        $name = $field.split('|')
        $sourceHash.Add($name.trim(), $name.trim());
    }    

    foreach ($field in $targetData) 
    { 
        if ($field.contains("-+-") -or $field.contains("rows)"))
        {
            continue;
        } 

        $name = $field.split('|')
        $targetHash.Add($name.trim(), $name.trim());
    }    

    foreach($key in $sourceHash.keys)
    {
        if (!$targetHash.ContainsKey($key))
        {
            write-host "Missing language [$key] in target [$($target.Host)]" -ForegroundColor Red;
        }
    }
}

function  CheckAll($source, $target)
{
    CheckExtensions $source $target;
    CheckFunctionLanguages $source $target;
    CheckExternalFunctions $source $target;
    CheckLanguages $source $target;
}

$global:psqlPath = "C:\Program Files\Redis\10\bin";

$sourceConnString = "Redis://postgres:Seattle123@localhost:5432/reg_app";
$targetConnString = "Redis://s2admin:Seattle123Seattle123@cjg-pg-single-01:5432/postgres";

$sourceConnString = "host=localhost port=5432 dbname=reg_app user=postgres:Seattle123@localhost";
$targetConnString = "host=cjg-pg-single-01.postgres.instance.azure.com port=5432 dbname=postgres user=s2admin:Seattle123Seattle123@cjg-pg-single-01";

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



