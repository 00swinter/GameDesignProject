using UnityEngine;
using UnityEngine.UI;

public class audioControl : MonoBehaviour
{
	[SerializeField]GameObject slider;


	[SerializeField]Slider musicSliderComponent;
	[SerializeField]AudioSource music;

	[SerializeField]Slider sfxSliderComponent;
	[SerializeField]AudioSource sfx;

	[SerializeField]GameObject ButtonIconMute;
	[SerializeField]GameObject ButtonIcon;
	private bool sliderShown = false;
	private bool musicCalled = false;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		if (!PlayerPrefs.HasKey("musicVolume"))
		{
			PlayerPrefs.SetFloat("musicVolume", 1);
			if (!PlayerPrefs.HasKey("sfxVolume"))
			{
				PlayerPrefs.SetFloat("sfxVolume", 1);
				Load();
			}
			else
			{
				Load();
			}
		}
		else
		{
			if (!PlayerPrefs.HasKey("sfxVolume")){
				PlayerPrefs.SetFloat("sfxVolume", 1);
				Load();
			}
			else
			{
				Load();
			}
		}
	}


	public void MusicVolume()
	{
		music.volume = Mathf.Clamp(musicSliderComponent.value, 0.0f,1.0f);
		bool icon = (music.volume==0.0f);
		ButtonIcon.SetActive(!icon);
		ButtonIconMute.SetActive(icon);
		musicCalled = true;
		Save();
	}

	public void SfxVolume()
	{
		sfx.volume = Mathf.Clamp(sfxSliderComponent.value, 0.0f, 1.0f);
		bool icon = (sfx.volume==0.0f);
		ButtonIcon.SetActive(!icon);
		ButtonIconMute.SetActive(icon);
		musicCalled = false;
		Save();
	}

	public void ToggleSlider()
	{
		sliderShown = !sliderShown;
		slider.SetActive(sliderShown);
	}

	private void Save()
	{
		if (musicCalled)
		{
			PlayerPrefs.SetFloat("musicVolume", musicSliderComponent.value);
		}
		else
		{
			PlayerPrefs.SetFloat("sfxVolume", sfxSliderComponent.value);
		}
	}

	private void Load()
	{
		musicSliderComponent.value = PlayerPrefs.GetFloat("musicVolume");
		sfxSliderComponent.value = PlayerPrefs.GetFloat("sfxVolume");
	}
}
