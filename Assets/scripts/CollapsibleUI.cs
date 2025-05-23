using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CollapsibleUI : MonoBehaviour
{
	public Button closeButton;
	public Button openButton;
	public RectTransform collapsiblePanel;
	public float animationDuration = 0.25f;

	private bool isExpanded = true;
	private Vector2 expandedSize;
	private Vector2 collapsedSize = new Vector2(45, 45);
	private CanvasGroup canvasGroup;

	void Start()
	{
		canvasGroup = collapsiblePanel.transform.GetComponentInChildren<CanvasGroup>();
		expandedSize = collapsiblePanel.sizeDelta;
		collapsedSize.y = expandedSize.y;
		closeButton.onClick.AddListener(TogglePanel);
		openButton.onClick.AddListener(TogglePanel);
	}

	public void TogglePanel()
	{
		StopAllCoroutines(); // stop ongoing animations
		if (isExpanded)
		{
			StartCoroutine(AnimatePanel(collapsiblePanel.sizeDelta, collapsedSize));
		}
		else
		{
			StartCoroutine(AnimatePanel(collapsiblePanel.sizeDelta, expandedSize));
		}
		canvasGroup.alpha = isExpanded ? 0f : 1f;
		canvasGroup.interactable = !isExpanded;
		canvasGroup.blocksRaycasts = !isExpanded;
		openButton.gameObject.SetActive(isExpanded);
		isExpanded = !isExpanded;
	}

	System.Collections.IEnumerator AnimatePanel(Vector2 from, Vector2 to)
	{
		float time = 0;
		while (time < animationDuration)
		{
			collapsiblePanel.sizeDelta = Vector2.Lerp(from, to, time / animationDuration);
			time += Time.deltaTime;
			yield return null;
		}
		collapsiblePanel.sizeDelta = to;
	}

}
