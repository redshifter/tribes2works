$BDMClient::clientVersion = 1.0;
$BDMClient::playingBDM = 0;

package BlasterDM_Client {

function setupObjHud(%gameType)
{
	$BDMClient::playingBDM = 0;

	if (%gameType $= "BlasterDMGame")
	{
		$BDMClient::playingBDM = 1;

		objectiveHud.setSeparators("44 81 100 137 195");

		objectiveHud.scoreLabel = new GuiTextCtrl() {
			profile = "GuiTextObjGreenLeftProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "4 3";
			extent = "41 16";
			visible = "1";
			text = "SCORE";
		};
		objectiveHud.yourScore = new GuiTextCtrl() {
			profile = "GuiTextObjGreenCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "47 3";
			extent = "35 16";
			visible = "1";
			text = "99999";
		};
		objectiveHud.yourRank = new GuiTextCtrl() {
			profile = "GuiTextObjGreenCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "84 3";
			extent = "17 16";
			visible = "1";
			text = "34";
		};
		objectiveHud.accLabel = new GuiTextCtrl() {
			profile = "GuiTextObjGreenCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "103 3";
			extent = "35 16";
			visible = "1";
			text = "ACC%";
		};
		objectiveHud.yourShots = new GuiTextCtrl() {
			profile = "GuiTextObjGreenCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "140 3";
			extent = "55 16";
			visible = "1";
			text = "9999/9999";
		};
		objectiveHud.yourAcc = new GuiTextCtrl() {
			profile = "GuiTextObjGreenCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "201 3";
			extent = "41 16";
			visible = "1";
			text = "34.24%";
		};

		objectiveHud.leadScoreLabel = new GuiTextCtrl() {
			profile = "GuiTextObjGoldLeftProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "4 19";
			extent = "41 16";
			visible = "1";
			text = "LEADER";
		};
		objectiveHud.leadScore = new GuiTextCtrl() {
			profile = "GuiTextObjGoldCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "47 19";
			extent = "35 16";
			visible = "1";
			text = "99999";
		};
		objectiveHud.leadRank = new GuiTextCtrl() {
			profile = "GuiTextObjGoldCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "84 19";
			extent = "17 16";
			visible = "1";
			text = "1";
		};
		objectiveHud.leadAccLabel = new GuiTextCtrl() {
			profile = "GuiTextObjGoldCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "103 19";
			extent = "35 16";
			visible = "1";
			text = "ACC%";
		};
		objectiveHud.leadShots = new GuiTextCtrl() {
			profile = "GuiTextObjGoldCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "140 19";
			extent = "55 16";
			visible = "1";
			text = "9999/9999";
		};
		objectiveHud.leadAcc = new GuiTextCtrl() {
			profile = "GuiTextObjGoldCenterProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "201 19";
			extent = "41 16";
			visible = "1";
			text = "34.24%";
		};

		objectiveHud.add(objectiveHud.scoreLabel);
		objectiveHud.add(objectiveHud.yourScore);
		objectiveHud.add(objectiveHud.yourRank);
		objectiveHud.add(objectiveHud.accLabel);
		objectiveHud.add(objectiveHud.yourShots);
		objectiveHud.add(objectiveHud.yourAcc);
		objectiveHud.add(objectiveHud.leadScoreLabel);
		objectiveHud.add(objectiveHud.leadScore);
		objectiveHud.add(objectiveHud.leadRank);
		objectiveHud.add(objectiveHud.leadAccLabel);
		objectiveHud.add(objectiveHud.leadShots);
		objectiveHud.add(objectiveHud.leadAcc);

		chatPageDown.setVisible(false);
	}
	else
		Parent::setupObjHud(%gameType);
}

function yourRankIs(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6)
{
	if( $BDMClient::playingBDM ) {
		%rank = detag(%a1);
		
		objectiveHud.yourRank.setValue(%rank); 
	}
	else
		Parent::yourRankIs(%msgType,%msgString,%a1,%a2,%a3,%a4,%a5,%a6);
}

function DispatchLaunchMode()
{
	echo("Adding message callbacks for Blaster DM...");

	addMessageCallback('MsgBlasterDMScore', bdmMyScore);
	addMessageCallback('MsgBlasterDMLeader', bdmLeader);

	Parent::DispatchLaunchMode();
}

function DisconnectedCleanup() {
	$BDMClient::playingBDM = 0;
	Parent::DisconnectedCleanup();
}

function bdmMyScore(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6) {
	objectiveHud.yourScore.setValue(%a1);
	objectiveHud.yourShots.setValue(%a2 @ "/" @ %a3);
	objectiveHud.yourAcc.setValue(%a4);
}

function bdmLeader(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6) {
	objectiveHud.leadScore.setValue(%a1);
	objectiveHud.leadShots.setValue(%a2 @ "/" @ %a3);
	objectiveHud.leadAcc.setValue(%a4);
}

function clientCmdMissionStartPhase2(%seq)
{
	// register with the server
	commandToServer('BlasterDMRegisterClient', $BDMClient::clientVersion);
	Parent::clientCmdMissionStartPhase2(%seq);
}

};

activatePackage(BlasterDM_Client);