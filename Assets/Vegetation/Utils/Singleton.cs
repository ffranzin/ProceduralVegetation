using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();

	protected void Awake()
	{
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
	}

	/// <summary>
	/// @maiandro
	///		Since this "MonoBehaviour" Singleton will be destroyed during a scene change if not marked as "DontDestroyOnLoad"
	/// and during the "OnDestroy" of it we set the "applicationIsQuitting" flag, which is STATIC, as true, to prevent a 
	/// "buggy ghost object that will stay on the Editor scene even after stopping playing the Application", this flag
	/// will remain with its value even after the scene changed.
	///		So if a scene A, that uses this singleton, loads a scene B that also uses it, the scene B will get a null
	/// reference to the instance of this Singleton because the "applicationIsQuitting" flag is still set to true. So for 
	/// every scene change, we reset the "applicationIsQuitting" flag back to false to prevent that from happening.
	/// </summary>
	private static void OnSceneChanged(Scene arg0, Scene arg1)
	{
		UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
		applicationIsQuitting = false;
	}

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock (_lock)
			{
				if (_instance == null)
				{
					_instance = (T)FindObjectOfType(typeof(T));

					MonoBehaviour[] ts = FindObjectsOfType<T>();
					if (ts.Length > 1)
					{
						foreach (MonoBehaviour t in ts)
							Debug.LogError("[Singleton]: " + t.GetType().Name + " found in GameObject " + t.gameObject.name);

						Debug.LogError("[Singleton] Something went really wrong " +
							" - there should never be more than 1 singleton!" +
							" Reopening the scene might fix it.");
						return _instance;
					}

					if (_instance == null)
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<T>();
						singleton.name = "(singleton) " + typeof(T).ToString();

						DontDestroyOnLoad(singleton);

						Debug.Log("[Singleton] An instance of " + typeof(T) +
							" is needed in the scene, so '" + singleton +
							"' was created with DontDestroyOnLoad.");
					}
					else
					{
						Debug.Log("[Singleton] Using instance already created: " +
							_instance.gameObject.name);
					}
				}

				return _instance;
			}
		}
	}


    public static bool HasInstance()
    {
        return _instance != null;
    }

	private static bool applicationIsQuitting = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	public void OnDestroy()
	{
		applicationIsQuitting = true;
	}
}