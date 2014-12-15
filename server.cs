$RPG::Dir = "config/scripts/mod/GameMode_Brawler/";
if(!isFile($RPG::Dir @ "server.cs"))
	$RPG::Dir = "Add-Ons/GameMode_Brawler/";

function Brawler_Init()
{
	exec("./scripts/main.cs");
}
Brawler_Init();

function brawlDebug(%msg)
{
	if($brawlDebug)
		echo(%msg);
}