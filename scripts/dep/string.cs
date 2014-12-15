function searchWords(%sourceString, %searchString)
{
	if(%searchString $= "")
		return -1;

	%ct = getWordCount(%sourceString);
	for(%i = 0; %i < %ct; %i++)
	{
		if(getWord(%sourceString, %i) $= %searchString)
			return %i;
	}
	return -1;
}