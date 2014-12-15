$Brawler::Damage::DefaultWeight = 94;
$Brawler::Damage::KnockbackModifier = 0.25; //Active modifier for knockback effect. This is necessary because Smash Bros. handles velocities differently, this is a simple scaling mechanism to make it more reasonable in Blockland.

function Player::BrawlerDamage(%this, %sourceObj, %damage, %knockback, %scale, %damPos, %vert, %applyZ)
{
	if(%damPos $= "")
		%damPos = %this.getPosition();

	brawlDebug("BRAWLER DEBUG: Damage done on " @ %this);
	brawlDebug("+--Source: " @ %sourceObj);
	brawlDebug("+--Damage: " @ %this.bDamPercent @ "-( " @ %damage @ ")->" @ %this.bDamPercent += %damage);
	brawlDebug("+--Knockback: " @ %knockback);
	brawlDebug("+--Knockback Scale: " @ %scale);
	brawlDebug("+--Position: " @ %damPos);

	//The rest here is for knockback, so we don't need to do anything in these cases:
	if(%damage <= 0 || !isObject(%sourceObj)) //no heals, no attacker, no service
		return;

	brawlDebug("+--Knockback Calculation");

	//For the time being, we're ripping Super Smash Bros. Melee/Brawl calculations, as detailed here: http://www.ssbwiki.com/knockback
	%weight = (%this.bWeight > 0 ? %this.bWeight : $Brawler::Damage::DefaultWeight);
	%c1 = (%this.bDamPercent / 10) + (%this.bDamPercent * %damage / 20);
	%c2 = %c1 * (200 / (%weight + 100)) * 1.4;
	%c3 = (%c2 + 18) * %scale;
	%knockbackEff = %c3 + %knockback;

	brawlDebug("   +--Weight: " @ %weight);
	brawlDebug("   +--c1-3: " @ %c1 SPC " " SPC %c2 SPC " " SPC %c3);
	brawlDebug("   +--Effect: " @ %knockbackEff);

	%sourcePos = %sourceObj.getPosition();

	%diff = VectorSub(%damPos, %sourcePos);
	brawlDebug("   +--Diff: " @ %diff);
	%norm = VectorNormalize(%diff);
	brawlDebug("   +--Norm: " @ %norm);
	%vel = VectorScale(%norm, %knockbackEff);
	brawlDebug("   +--Vel: " @ %vel);
	%vel = VectorScale(%vel, $Brawler::Damage::KnockbackModifier);
	brawlDebug("   +--Vel Post-Modifier: " @ %vel);
	brawlDebug("   +--Vertical: " @ %vert SPC (%applyZ ? ", apply Z knockback" : ", no Z knockback"));
	if(!%applyZ)
		%vel = getWords(%vel, 0, 1) SPC 0;
	%vel = VectorAdd(%vel, "0 0" SPC %vert);
	brawlDebug("   +--Final Velocity: " @ %vel);
	%this.addVelocity(%vel);
}

function Player::GetBrawlerDamage(%this)
{
	return %this.bDamPercent;
}

function BrawlerSolveNegation_Time(%baseDamage, %startTime, %negationPeriod, %minDamage)
{
	%negationTime = (%negationPeriod - ($Sim::Time - %startTime));
	if(%negationTime <= 0)
		return %minDamage + 0;

	%eff = %negationTime / %negationPeriod;
	return %baseDamage * %eff;
}