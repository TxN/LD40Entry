using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BBTManager : MonoBehaviour {

	[System.Serializable]
	public class BBTMessage {
		public string Text = "";
		public bool Important = false;
		public float ShowTime = 0f;
	}

	enum StripeState {
		Retracted,
		Shown,
		MovingUp,
		MovingDown
	}

	public Text TextField;
	public RectTransform BBTTransform;
	public Vector3 RetractedPos;
	public Vector3 ShownPos;

	StripeState _state = StripeState.Retracted;
	Sequence _seq = null;
	List<BBTMessage> _messageQueue = new List<BBTMessage>();
	BBTMessage _curMessage = null;
	float _moveTime = 0.7f;

	void Start() {
		Init();
	}

	void Init() {
	}

	public void ShowBBT(string text, float time, bool important = false) {
		ShowBBT(new BBTMessage {
			Text = text,
			Important = important,
			ShowTime = time
		});
	}

	public void ShowBBT(BBTMessage message) {
		if ( message.Important ) {
			_messageQueue.Insert(0, message);
		} else {
			_messageQueue.Add(message);
		}
		ProcessState();
	}

	void ProcessState() {
		if ( _curMessage == null && _messageQueue.Count > 0) {
			_curMessage = _messageQueue[0];
			_messageQueue.RemoveAt(0);
		}

		if ( _curMessage == null ) {
			return;
		}

		if ( _state == StripeState.Retracted ) {
			Show();
		}

	}

	void Show() {
		if (_curMessage == null) {
			ProcessState();
			return;
		}
		BBTTransform.gameObject.SetActive(true);
		TextField.text = _curMessage.Text;
		_state = StripeState.MovingDown;
		_seq = TweenHelper.ReplaceSequence(_seq);
		_seq.Append( BBTTransform.DOLocalMove(ShownPos, _moveTime));
		_seq.AppendCallback(() => { _state = StripeState.Shown; });
		_seq.AppendInterval(_curMessage.ShowTime);
		_seq.AppendCallback(() => { _state = StripeState.MovingUp; });
		_seq.Append(BBTTransform.DOLocalMove(RetractedPos, _moveTime));
		_seq.AppendCallback(() => { _state = StripeState.Retracted; _curMessage = null; BBTTransform.gameObject.SetActive(false); ProcessState();  });
	}

}
