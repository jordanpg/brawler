//Brawler Control System
//Author: ottosparks
//As of December 14, 2014

////This code controls the mechanics of all brawlers.
////Its role is to avoid repetitive control code and to ensure mechanical consistency for all playstyles.
////Of course, this modularity sacrifices some individuality if not accounted for.
////For this reason, it's important that we load this up with as much open-ended poopy as possible.

////Move set control names are as follows:
//*Left-click/activate (trigger 0): Standard
//*Right-click/jet (trigger 4): Secondary
//*Space/jump (trigger 2): Jump
//	Double-jump control triggered when pressed again within $Brawler::Control::DoubleJumpPeriod seconds: DoubleJump

////Conventions:
//*A player's class is determined by the brawler_class variable on either their datablock or their player.
//	Such a value on the player will take precedence over one on their datablock.
//*Move sets are defined using the $Brawler::Moveset[classname, control] global array.
//*Additional move set rules are defined using the $Brawler::MovesetRule[classname, rulename, control] global array.
//	e.g. the UnCall rule, which will call ability phase -1 (deactivate) for the move set control when the opposite trigger value occurs. (ie, trigger 0 is released on a Standard control)
//*For trigger-based controls, you may also append "Release" to the end of the control name to have a control which activates upon releasing the trigger, rather than upon activating it.
//	This can be used, for example, for a complex move that requires a charge-up period, wherein the move is actually executed when the charging trigger is released.


$Brawler::Control::DoubleJumpPeriod = 0.2;

package Brawler_BrawlerControl
{
	function Armor::onTrigger(%db, %this, %slot, %val)
	{
		parent::onTrigger(%db, %this, %slot, %val);

		if(%db.brawler_class $= "" && %this.brawler_class $= "")
			return;

		%class = (%this.brawler_class !$= "" ? %this.brawler_class : (%db.brawler_class !$= "" ? %db.brawler_class : ""));

		if(%class $= "")
			return;

		switch(%slot)
		{
			case 0:
				if(%val)
				{
					if($Brawler::MovesetRule[%class, "UnCall", "StandardRelease"])
					{
						if(!isValidAbility($Brawler::Moveset[%class, "StandardRelease"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "StandardRelease"]))
							return;

						$Brawler::Moveset[%class, "StandardRelease"].aPhase(%this, -1);
					}

					if(isValidAbility($Brawler::Moveset[%class, "Standard"]))
						%this.activateAbility($Brawler::Moveset[%class, "Standard"], 0);
				}
				else
				{
					if($Brawler::MovesetRule[%class, "UnCall", "Standard"])
					{
						if(!isValidAbility($Brawler::Moveset[%class, "Standard"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "Standard"]))
							return;

						$Brawler::Moveset[%class, "Standard"].aPhase(%this, -1);
					}

					if(isValidAbility($Brawler::Moveset[%class, "StandardRelease"]))
						%this.activateAbility($Brawler::Moveset[%class, "StandardRelease"], 0);
				}
			case 2:
				if(%val)
				{
					if($Sim::Time - %this.bLastJump >= $Brawler::Control::DoubleJumpPeriod)
					{
						if($Brawler::MovesetRule[%class, "UnCall", "JumpRelease"])
						{
							if(!isValidAbility($Brawler::Moveset[%class, "JumpRelease"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "JumpRelease"]))
								return;

							$Brawler::Moveset[%class, "JumpRelease"].aPhase(%this, -1);
						}

						if(isValidAbility($Brawler::Moveset[%class, "Jump"]))
							%this.activateAbility($Brawler::Moveset[%class, "Jump"], 0);

						%this.bLastJump = $Sim::Time;
						%this.bLastJumpDouble = false;
					}
					else if(!%this.bLastJumpDouble)
					{
						if(isValidAbility($Brawler::Moveset[%class, "DoubleJump"]))
							%this.activateAbility($Brawler::Moveset[%class, "DoubleJump"], 0);
						%this.bLastJumpDouble = true;
						%this.bLastJump = $Sim::Time;
					}
					else
					{
						if($Brawler::MovesetRule[%class, "UnCall", "DoubleJumpRelease"])
						{
							if(!isValidAbility($Brawler::Moveset[%class, "DoubleJumpRelease"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "DoubleJumpRelease"]))
								return;

							$Brawler::Moveset[%class, "DoubleJumpRelease"].aPhase(%this, -1);
						}
					}
				}
				else
				{
					if(!%this.bLastJumpDouble)
					{
						if($Brawler::MovesetRule[%class, "UnCall", "Jump"])
						{
							if(!isValidAbility($Brawler::Moveset[%class, "Jump"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "Jump"]))
								return;

							$Brawler::Moveset[%class, "Jump"].aPhase(%this, -1);
						}

						if(isValidAbility($Brawler::Moveset[%class, "JumpRelease"]))
							%this.activateAbility($Brawler::Moveset[%class, "JumpRelease"], 0);
					}
					else
					{
						if($Brawler::MovesetRule[%class, "UnCall", "DoubleJump"])
						{
							if(!isValidAbility($Brawler::Moveset[%class, "DoubleJump"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "DoubleJump"]))
								return;

							$Brawler::Moveset[%class, "DoubleJump"].aPhase(%this, -1);
						}

						if(isValidAbility($Brawler::Moveset[%class, "DoubleJumpRelease"]))
							%this.activateAbility($Brawler::Moveset[%class, "DoubleJumpRelease"], 0);
					}
				}
			case 4:
				if(%val)
				{
					if($Brawler::MovesetRule[%class, "UnCall", "SecondaryRelease"])
					{
						if(!isValidAbility($Brawler::Moveset[%class, "SecondaryRelease"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "SecondaryRelease"]))
							return;

						$Brawler::Moveset[%class, "SecondaryRelease"].aPhase(%this, -1);
					}

					if(isValidAbility($Brawler::Moveset[%class, "Secondary"]))
						%this.activateAbility($Brawler::Moveset[%class, "Secondary"], 0);
				}
				else
				{
					if($Brawler::MovesetRule[%class, "UnCall", "Secondary"])
					{
						if(!isValidAbility($Brawler::Moveset[%class, "Secondary"]) || %this.getActiveAbility(0) != nameToID($Brawler::Moveset[%class, "Secondary"]))
							return;

						$Brawler::Moveset[%class, "Secondary"].aPhase(%this, -1);
					}

					if(isValidAbility($Brawler::Moveset[%class, "SecondaryRelease"]))
						%this.activateAbility($Brawler::Moveset[%class, "SecondaryRelease"], 0);
				}
		}
	}
};
activatePackage(Brawler_BrawlerControl);