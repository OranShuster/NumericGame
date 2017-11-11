using UnityEngine;

public class GameManagerLoader : MonoBehaviour 
{
	public GameObject GameManager;
        
        
	void Awake ()
	{
		if (global::GameManager.Instance== null)
			Instantiate(GameManager);
	}
}