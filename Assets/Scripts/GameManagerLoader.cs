using UnityEngine;

public class GameManagerLoader : MonoBehaviour 
{
	public GameObject Instance;
        
        
	void Awake ()
	{
		if (GameManager.Instance== null)
			Instantiate(Instance);
	}
}