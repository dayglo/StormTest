#pragma strict
static var score : int;
private var scp : GameObject;

static var lblScore : UILabel; 

function Start() {

scp = GameObject.FindWithTag("Score");

lblScore = scp.GetComponent(UILabel);

}
static function addscore(addscore : int) {

if (lblScore) {
score = parseInt(lblScore.text);
//scp = lblScore.text;

score=score + addscore;

lblScore.text = score.ToString();
//Debug.Log (score);

}

}