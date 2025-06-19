using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class menuInteractions : MonoBehaviour
{

	public GameObject helpPanel;
	public GameObject pausePanel;
	public GameObject energyPanel;
	public GameObject shopPanel;
	public GameObject housePanel;
	public GameObject creditPanel;
	public int gameSceneIndex = 1;
	public bool isPaused = false;
	public bool isEnergy = false;
	public bool isShop = false;
	public bool isHelp = true;
	public bool isHouses = false;
	public bool isCredit = false;
void Start()
    {
        
    }

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isEnergy)
				ToggleEnergyMenu();
			else if (isShop)
				ToggleShopMenu();
			else if (isHelp)
				ToggleHelpMenu();
			else if (isHouses)
				ToggleHouseMenu();
			else if (isCredit)
				ToggleCreditMenu();
			else
				TogglePauseMenu();
		}
	}
	public void TogglePauseMenu()
	{
		isPaused = !isPaused;
		pausePanel.SetActive(isPaused);
		Time.timeScale = isPaused ? 0 : 1;
	}
	public void ToggleEnergyMenu()
	{
		isEnergy = !isEnergy;
		energyPanel.SetActive(isEnergy);
	}

	public void ToggleShopMenu()
	{
		isShop = !isShop;
		shopPanel.SetActive(isShop);
	}
	public void ToggleHelpMenu()
	{
		isHelp = !isHelp;
		helpPanel.SetActive(isHelp);
	}

	public void ToggleHouseMenu()
	{
		isHouses = !isHouses;
		housePanel.SetActive(isHouses);
	}

	public void ToggleCreditMenu()
	{
		isCredit = !isCredit;
		creditPanel.SetActive(isCredit);
	}
	public void QuitGame()
	{
		Application.Quit();
		Debug.Log("Quit Game");
	}

	public void StartGame()
	{
		SceneManager.LoadScene(gameSceneIndex);
	}

	public void QuitToMainMenu()
	{
		SceneManager.LoadScene(0);
	}

}
