using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class RaceCountdown : MonoBehaviour {
	const int TEXT_MAX_SIZE = 200;
	const int TEXT_MIN_SIZE = 60;

	CanvasGroup _cg = null;
	Text _text      = null;
	Sequence _seq   = null;

	public void StartSequence() {
		gameObject.SetActive(true);
		_cg = GetComponent<CanvasGroup>();
		_text = GetComponent<Text>();
		_cg.alpha = 1f;
		_text.fontSize = TEXT_MAX_SIZE;
		_text.text = "3";

		_seq = TweenHelper.ReplaceSequence(_seq, false);
		_seq.Append(transform.DOScale(0.25f, 1f));
		_seq.Insert(0.65f, _cg.DOFade(0, 0.3f));
		_seq.AppendCallback( () => { _text.text = "2"; _cg.alpha = 1f; transform.localScale = Vector3.one; } );

		_seq.AppendInterval(0.1f);
		_seq.Append(transform.DOScale(0.25f, 1f));
		_seq.Insert(1.75f, _cg.DOFade(0, 0.3f));
		_seq.AppendCallback(() => { _text.text = "1"; _cg.alpha = 1f; transform.localScale = Vector3.one; });

		_seq.AppendInterval(0.1f);
		_seq.Append(transform.DOScale(0.25f, 1f));
		_seq.Insert(2.85f, _cg.DOFade(0, 0.3f));
		_seq.AppendCallback(() => { _text.text = "GO!!!"; _cg.alpha = 1f; transform.localScale = Vector3.one; });

		_seq.Append(transform.DOShakePosition(0.75f, 30, 30, 90));
		_seq.Insert(3.4f, _cg.DOFade(0, 0.35f));

		_seq.AppendCallback(() => { gameObject.SetActive(false); });
	}

	void OnDestroy() {
		TweenHelper.ResetSequence(_seq);
	}
}
