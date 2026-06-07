using UnityEngine;

public class ConveyorScroll : MonoBehaviour
{

    // Attributes

    [SerializeField] private float speed = 2.0f;
    private Material mat;
    private Vector2 offset;

    private AudioSource audioSource;
    private bool audioPaused = false;

    // Methods

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat = Instantiate(mat);
        GetComponent<Renderer>().material = mat;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0.01f)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                audioPaused = true;
            }
            return;
        }
        else
        {
            if (audioSource != null && !audioSource.isPlaying && audioPaused)
            {
                audioSource.UnPause();
                audioPaused = false;
            }
        }

        offset.y -= speed * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }

}
