using UnityEngine;
using System.IO;

public class Display_Image : MonoBehaviour
{
    public MeshRenderer plane;
    public float minSize = (float)0.3;
    public float maxSize = (float)2.0;
    // Start is called before the first frame update
    void Awake()
    {
        InstantiateImage("", Random.Range(minSize, maxSize));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InstantiateImage(string img, float img_width)
    {
        if (img.Length == 0)
        {
            var files = Directory.GetFiles(Application.streamingAssetsPath, "*.jpg", SearchOption.AllDirectories);
            img = files[Random.Range(0, files.Length)];
        }

        byte[] bytes = File.ReadAllBytes(img);
        Texture2D texture = new Texture2D(2, 2);
        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        float w = texture.width;
        float h = texture.height;
        float img_height = img_width * (h / w);

        this.gameObject.GetComponentInChildren<Renderer>().material.mainTexture = texture;
        this.gameObject.transform.localScale = new Vector3(img_width, img_height, 1);
    }
}
