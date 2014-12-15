$Brawler::Fighter::DoubleJumpPeriod = 0.2;
package Brawler_Fighter
{
	function Armor::onTrigger(%db, %this, %slot, %val)
	{
		parent::onTrigger(%db, %this, %slot, %val);

		if(%db.brawler_class !$= "Fighter" && %this.brawler_class !$= "Fighter")
			return;

		switch(%slot)
		{
			case 0:
				if(%val)
				{
					if(isObject(%this.getMountedImage(0)) || isObject(%this.getActiveAbility(0)))
						return;
					%this.activateAbility(AbilityFighter_Punch, 0);
				}
				else
				{
					if(%this.getActiveAbility(0) == nameToID(AbilityFighter_Punch))
						AbilityFighter_Punch.aPhase(%this, -1);
				}
			case 2:
				if(%val)
				{
					if($Sim::Time - %this.lastJump < $Brawler::Fighter::DoubleJumpPeriod)
						%this.activateAbility(AbilityFighter_Butterfly, 0);
					else
						%this.lastJump = $Sim::Time;
				}
		}
	}

	function serverCmdUseInventory(%this, %slot)
	{
		if(!isObject(%player = %this.player))
			return parent::serverCmdUseInventory(%this, %slot);

		if(%player.db.brawler_class !$= "Fighter" && %player.brawler_class !$= "Fighter")
			return parent::serverCmdUseInventory(%this, %slot);

		switch(%slot)
		{
			case 1:
				%player.activateAbility(AbilityFighter_Lunge, 0);
		}
	}
};
activatePackage(Brawler_Fighter);

//------SPECIAL 1: LUNGE------
$Brawler::Fighter::LungeSpeedScale = 30;
$Brawler::Fighter::LungeHitTolerance = 0.3;
$Brawler::Fighter::LungeDistance = 12;
$Brawler::Fighter::LungeMaxTime = 1.5;
$Brawler::Fighter::LungeKnockbackScale = 1;
$Brawler::Fighter::LungeDamageTimeout = 0.5;
$Brawler::Fighter::LungeDamage = 25;
$Brawler::Fighter::LungeKnockback = 25;
$Brawler::Fighter::LungeCooldown = 1;
$Brawler::Fighter::LungeTerminatorScale = 0.5;
$Brawler::Fighter::LungeTerminatorSpeed = 1;
$Brawler::Fighter::LungeVerticalEff = 2.5;
$Brawler::Fighter::LungePreHang = 0.5;

if(isObject(AbilityFighter_Lunge))
	AbilityFighter_Lunge.delete();
new ScriptObject(AbilityFighter_Lunge)
{
	class = "Ability";

	abilityCooldown = 10; //We're going to handle cooldowns manually here.
	abilityName = "Fighter Lunge";
	abilityNoCancel = true;
	abilityNoImages = true;

	phase0 = "Init";
	phaseCall0 = true;
	phaseTime0 = 0;
	phaseNext0 = 1;

	phase1 = "Action";
	phaseCall1 = true;
	phaseTime1 = 20;
	phaseNext1 = 1;

	phase2 = "Attack";
	phaseCall2 = true;
	phaseTime2 = 20;
	phaseNext2 = 2;

	phases = 3;

	debug = false;
};

function AbilityFighter_Lunge::Init(%this, %obj, %slot)
{
	%obj.abilityInfo[%slot, 0] = %obj.getForwardVector();
	%obj.abilityInfo[%slot, 1] = %obj.getHackPosition();
	%obj.abilityInfo[%slot, 2] = $Sim::Time;

	%obj.setVelocity("0 0 0");
}

function AbilityFighter_Lunge::Action(%this, %obj, %slot)
{
	%obj.setVelocity("0 0 0.4");

	if($Sim::Time - %obj.abilityInfo[%slot, 2] >= $Brawler::Fighter::LungePreHang)
	{
		%vel = VectorScale(%obj.abilityInfo[%slot, 0], $Brawler::Fighter::LungeSpeedScale);
		%obj.setVelocity(%vel);
		%obj.abilityInfo[%slot, 2] = $Sim::Time;
		%this.aPhase(%obj, 2);
	}
}

function AbilityFighter_Lunge::Attack(%this, %obj, %slot)
{
	%pos = %obj.getHackPosition();
	%vel = %obj.getVelocity();
	%vX = getWord(%vel, 0);
	%vY = getWord(%vel, 1);
	%vZ = getWord(%vel, 2);
	%obj.setVelocity(%vX SPC %vY SPC 0.4);

	initContainerRadiusSearch(%pos, $Brawler::Fighter::LungeHitTolerance, $TypeMasks::PlayerObjectType);
	while(isObject(%o = containerSearchNext()))
	{
		if(nameToID(%o) == nameToID(%obj))
			continue;

		if(($nocheck || minigameCanDamage(%obj, %o) == 1) && $Sim::Time - %o.aLungeLast > $Brawler::Fighter::LungeDamageTimeout)
		{
			%o.BrawlerDamage(%obj, $Brawler::Fighter::LungeDamage, $Brawler::Fighter::LungeKnockback, $Brawler::Fighter::LungeKnockbackScale, "", $Brawler::Fighter::LungeVerticalEff);

			%o.aLungeLast = $Sim::Time;
		}
	}

	%dist = VectorDist(%obj.abilityInfo[%slot, 1], %pos);
	if(%dist >= $Brawler::Fighter::LungeDistance || $Sim::Time - %obj.abilityInfo[%slot, 2] >= $Brawler::Fighter::LungeMaxTime || VectorLen(%vel) <= $Brawler::Fighter::LungeTerminatorSpeed)
		%this.aPhase(%obj, -1);
}

function AbilityFighter_Lunge::onInactive(%this, %obj, %slot)
{
	%obj.abilityInfo[%slot, 0] = "";
	%obj.abilityInfo[%slot, 1] = "";
	%obj.abilityInfo[%slot, 2] = "";

	%obj.setAbilityCooldown(%this, $Brawler::Fighter::LungeCooldown);
	%new = VectorScale(%obj.getVelocity(), $Brawler::Fighter::LungeTerminatorScale);
	echo(%new);
	%obj.setVelocity(%new);

	parent::onInactive(%this, %obj, %slot);
}

function serverCmdLunge(%this)
{
	if(!isObject(%this.player))
		return;

	%this.player.activateAbility(AbilityFighter_Lunge, 0);
}


//------STANDARD ATTACK: PUNCH------
$Brawler::Fighter::PunchVelFade = 0.25;
$Brawler::Fighter::PunchRadius = 0.25;
$Brawler::Fighter::PunchDisplacement = 1;
$Brawler::Fighter::PunchBaseDamage = 5;
$Brawler::Fighter::PunchNegationPeriod = 2.5;
$Brawler::Fighter::PunchMinDamage = 0.05;
$Brawler::Fighter::PunchKnockback = 25;
$Brawler::Fighter::PunchKnockbackScale = 1;

if(isObject(AbilityFighter_Punch))
	AbilityFighter_Punch.delete();
new ScriptObject(AbilityFighter_Punch)
{
	class = "Ability";

	abilityCooldown = 1;
	abilityName = "Fighter Punch";
	abilityNoCancel = true;
	abilityNoImages = true;

	phase0 = "Init";
	phaseCall0 = true;
	phaseTime0 = 0;
	phaseNext0 = 1;

	phase1 = "Attack";
	phaseCall1 = true;
	phaseTime1 = 100;
	phaseNext1 = 1;

	phases = 2;

	debug = false;
};

function AbilityFighter_Punch::Init(%this, %obj, %slot)
{
	%obj.abilityInfo[%slot, 0] = $Sim::Time;
}

function AbilityFighter_Punch::Attack(%this, %obj, %slot)
{
	%vel = %obj.getVelocity();
	%scale = VectorScale(%vel, $Brawler::Fighter::PunchVelFade);
	%obj.setVelocity(getWords(%scale, 0, 1) SPC getWord(%vel, 2)); //Keep Z velocity.

	%damage = BrawlerSolveNegation_Time($Brawler::Fighter::PunchBaseDamage, %obj.abilityInfo[%slot, 0], $Brawler::Fighter::PunchNegationPeriod, $Brawler::Fighter::PunchMinDamage);

	%eye = %obj.getEyeVector();
	%add = VectorScale(%eye, $Brawler::Fighter::PunchDisplacement);
	%searchPos = VectorAdd(%obj.getHackPosition(), %add);
	initContainerRadiusSearch(%searchPos, $Brawler::Fighter::PunchRadius, $TypeMasks::PlayerObjectType);
	while(isObject(%o = containerSearchNext()))
	{
		if(nameToID(%o) == nameToID(%obj))
			continue;

		if(($nocheck || minigameCanDamage(%obj, %o) == 1))
			%o.BrawlerDamage(%obj, %damage, $Brawler::Fighter::PunchKnockback, $Brawler::Fighter:PunchKnockbackScale);
	}

	%obj.playThread(3, activate2);
}

function AbilityFighter_Punch::onInactive(%this, %obj, %slot)
{
	%obj.abilityInfo[%slot, 0] = "";

	parent::onInactive(%this, %obj, %slot);
}


//------DOUBLE-JUMP: BUTTERFLY LUNGE------
$Brawler::Fighter::ButterflyPreHang = 0.5;
$Brawler::Fighter::ButterflyDamage_Start = 15;
$Brawler::Fighter::ButterflyKnockback_Start = 35;
$Brawler::Fighter::ButterflyKnockbackScale_Start = 0.75;
$Brawler::Fighter::ButterflyDamage_Fly = 5;
$Brawler::Fighter::ButterflyKnockback_Fly = 25;
$Brawler::Fighter::ButterflyKnockbackScale_Fly = 1;
$Brawler::Fighter::ButterflyHitRadius = 0.25;
$Brawler::Fighter::ButterflyVerticalEff_Start = 5;
$Brawler::Fighter::ButterflyVerticalEff_Fly = 0;
$Brawler::Fighter::ButterflyPeriod = 2.5;
$Brawler::Fighter::ButterflySpeed = 5;
$Brawler::Fighter::ButterflyExitScale = 0.8;
$Brawler::Fighter::ButterflyCooldown = 2.5;

if(isObject(AbilityFighter_Butterfly))
	AbilityFighter_Butterfly.delete();
new ScriptObject(AbilityFighter_Butterfly)
{
	class = "Ability";

	abilityCooldown = 10;
	abilityName = "Fighter Butterfly Lunge";
	abilityNoCancel = true;
	abilityNoImages = true;

	phase0 = "Init";
	phaseCall0 = true;
	phaseTime0 = 0;
	phaseNext0 = 1;

	phase1 = "Action";
	phaseCall1 = true;
	phaseTime1 = 20;
	phaseNext1 = 1;

	phase2 = "Attack";
	phaseCall2 = true;
	phaseTime2 = 50;
	phaseNext2 = 2;

	phases = 3;

	debug = false;
};

function AbilityFighter_Butterfly::Init(%this, %obj, %slot)
{
	%obj.abilityInfo[%slot, 0] = $Sim::Time;

	%obj.setVelocity("0 0 0");
}

function AbilityFighter_Butterfly::Action(%this, %obj, %slot)
{
	%obj.setVelocity("0 0 0.4");

	if($Sim::Time - %obj.abilityInfo[%slot, 0] >= $Brawler::Fighter::ButterflyPreHang)
	{
		%pos = %obj.getHackPosition();
		initContainerRadiusSearch(%pos, $Brawler::Fighter::ButterflyHitRadius, $TypeMasks::PlayerObjectType);
		while(isObject(%o = containerSearchNext()))
		{
			if(nameToID(%o) == nameToID(%obj))
				continue;

			if(($nocheck || minigameCanDamage(%obj, %o) == 1))
				%o.BrawlerDamage(%obj, $Brawler::Fighter::ButterflyDamage_Start, $Brawler::Fighter::ButterflyKnockback_Start, $Brawler::Fighter::ButterflyKnockbackScale_Start, "", $Brawler::Fighter::ButterflyVerticalEff_Start);
		}

		%obj.abilityInfo[%slot, 0] = $Sim::Time;
		%this.aPhase(%obj, 2);
	}
}

function AbilityFighter_Butterfly::Attack(%this, %obj, %slot)
{
	%eye = %obj.getEyeVector();
	%norm = VectorNormalize(%eye);
	%vel = VectorScale(%norm, $Brawler::Fighter::ButterflySpeed);
	%obj.setVelocity(%vel);

	initContainerRadiusSearch(%obj.getHackPosition(), $Brawler::Fighter::ButterflyHitRadius, $TypeMasks::PlayerObjectType);
	while(isObject(%o = containerSearchNext()))
	{
		if(nameToID(%o) == nameToID(%obj))
			continue;

		if(($nocheck || minigameCanDamage(%obj, %o) == 1))
			%o.BrawlerDamage(%obj, $Brawler::Fighter::ButterflyDamage_Fly, $Brawler::Fighter::ButterflyKnockback_Fly, $Brawler::Fighter::ButterflyKnockbackScale_Fly, "", $Brawler::Fighter::ButterflyVerticalEff_Fly);
	}

	if($Sim::Time - %obj.abilityInfo[%slot, 0] >= $Brawler::Fighter::ButterflyPeriod)
		%this.aPhase(%obj, -1);
}

function AbilityFighter_Butterfly::onInactive(%this, %obj, %slot)
{
	%obj.abilityInfo[%slot, 0] = "";

	%obj.setAbilityCooldown(%this, $Brawler::Fighter::ButterflyCooldown);
	%new = VectorScale(%obj.getVelocity(), $Brawler::Fighter::ButterflyExitScale);
	%obj.setVelocity(%new);

	parent::onInactive(%this, %obj, %slot);
}