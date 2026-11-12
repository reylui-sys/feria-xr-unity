using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyUI : MonoBehaviour
{
	[Header("Botones de dificultad")]
	public Button easyButton;
	public Button normalButton;
	public Button hardButton;

	[Header("Feedback visual")]
	public Color selectedColor = Color.yellow;
	public Color defaultColor = Color.white;

	[Header("Descripción")]
	public TextMeshProUGUI descriptionText;

	private readonly string[] descriptions = {
		"Fácil: 90 segundos, obstáculos lentos",
		"Normal: 60 segundos, velocidad estándar",
		"Difícil: 45 segundos, obstáculos rápidos, puntos x2"
	};

	void Start()
	{
		easyButton.onClick.AddListener(() => SelectDifficulty(0));
		normalButton.onClick.AddListener(() => SelectDifficulty(1));
		hardButton.onClick.AddListener(() => SelectDifficulty(2));

		// Normal seleccionado por defecto
		SelectDifficulty(1);
	}

	void SelectDifficulty(int level)
	{
		DifficultyManager.Instance.SetDifficulty(level);

		// Resetear colores
		easyButton.GetComponent<Image>().color = defaultColor;
		normalButton.GetComponent<Image>().color = defaultColor;
		hardButton.GetComponent<Image>().color = defaultColor;

		// Resaltar el seleccionado
		Button selected = level == 0 ? easyButton :
						  level == 1 ? normalButton : hardButton;
		selected.GetComponent<Image>().color = selectedColor;

		if (descriptionText != null)
			descriptionText.text = descriptions[level];
	}
}