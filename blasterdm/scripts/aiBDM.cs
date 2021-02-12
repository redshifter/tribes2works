function BlasterDMGame::AIInit(%game)
{
	//call the default AIInit() function
	AIInit();
}

function BlasterDMGame::onAIRespawn(%game, %client)
{
   //add the default task
	if (!%client.defaultTasksAdded)
	{
	   %client.defaultTasksAdded = true;
	   %client.addTask(AIEngageTask);
	   %client.addTask(AIPatrolTask);
	}

	//set the inv flag
	%client.spawnUseInv = false;
}

function BlasterDMGame::RSAI_heHitMe(%game, %client) {
   if( !%client.isAIControlled() )
      return;

   if (%client.lastDamagedBy != %client)
      %client.setEngageTarget(%client.lastDamagedBy);
}