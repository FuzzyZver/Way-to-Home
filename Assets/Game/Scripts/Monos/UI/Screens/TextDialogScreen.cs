using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TextDialogScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _characterName;
    [SerializeField] private TextMeshProUGUI _dialogText;
    [SerializeField] private Transform _transform;
    [SerializeField] private Image _background;

    // AnimationProperties
    private float _moveDuration = 0.8f;
    private float _typingDuration = 1.5f;
    private float _startPos = -200f;
    private float _endPos = 120f;

    private Sequence _currentSequence;

    private void Awake()
    {
        var pos = _transform.localPosition;
        pos.y = _startPos;
        _transform.localPosition = pos;

        Color bg = _background.color;
        bg.a = 0f;
        _background.color = bg;

        _dialogText.text = "";
        _characterName.text = "";
    }

    public void StartDialog()
    {
        _currentSequence?.Kill();

        _currentSequence = DOTween.Sequence();

        _currentSequence.Join(
            _transform.DOLocalMoveY(_endPos, _moveDuration).SetEase(Ease.OutBack)
        );

        _currentSequence.Join(
            _background.DOFade(1f, _moveDuration)
        );
    }

    public void Speech(string characterName, string text)
    {
        _currentSequence?.Kill();

        _characterName.text = characterName;
        _dialogText.text = "";

        int totalChars = text.Length;

        _currentSequence = DOTween.Sequence();

        _currentSequence.Append(
            DOTween.To(
                () => 0,
                x => _dialogText.text = text.Substring(0, x),
                totalChars,
                _typingDuration
            ).SetEase(Ease.Linear)
        );
    }

    public void EndDialog()
    {
        _currentSequence?.Kill();

        _currentSequence = DOTween.Sequence();

        _currentSequence.Join(
            _transform.DOLocalMoveY(_startPos, _moveDuration).SetEase(Ease.InBack)
        );

        _currentSequence.Join(
            _background.DOFade(0f, _moveDuration)
        );

        _currentSequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}