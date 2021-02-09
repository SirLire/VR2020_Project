using UnityEngine;
using System.IO;

public class Display_Image : MonoBehaviour
{
    public MeshRenderer plane;
    public float minSize = 0.3f;
    public float maxSize = 2.0f;
    public float maxHeight = 3.0f;
    public GameObject latoQuadro;
    public GameObject angoloQuadro;
    public float spessoreCornice = 0.00f;
    public float offsetCornice = 0.00f;
    


    void Awake()
    {
        InstantiateImage("", 0);
    }

    float InstantiateImage(string img, float img_width)
    {
        float img_true_width = img_width;
        if (img.Length == 0)
        {
            var files = Directory.GetFiles(Application.streamingAssetsPath, "*.jpg", SearchOption.AllDirectories);
            img = files[Random.Range(0, files.Length)];
        }
        if (img_true_width == 0)
        {
            img_true_width = Random.Range(minSize, maxSize);
        }

        byte[] bytes = File.ReadAllBytes(img);
        Texture2D texture = new Texture2D(2, 2);
        texture.filterMode = FilterMode.Trilinear;
        texture.LoadImage(bytes);

        float w = texture.width;
        float h = texture.height;
        float img_height = img_true_width * (h / w);

        if (img_height > maxHeight)
        {
            img_height = maxHeight;
            img_true_width = img_height * (w / h);
        }

        this.gameObject.GetComponentInChildren<Renderer>().material.mainTexture = texture;
        this.gameObject.transform.localScale = new Vector3(img_true_width, img_height, 1);

        AggiungiCornice(img_true_width, img_height);

        return img_true_width;
    }

    void AggiungiCornice (float width, float height)
    {
        GameObject new_Target;
        Vector3 posizioneQuadro = this.gameObject.transform.position;
        
        //lati sinistro e destro
        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x - (width + spessoreCornice) / 2, posizioneQuadro.y, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, height, 1);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x + (width + spessoreCornice) / 2, posizioneQuadro.y, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, height, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        new_Target.transform.parent = this.transform;

        //lati superiore e inferiore 
        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x, posizioneQuadro.y - (height + spessoreCornice) / 2, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, width, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x, posizioneQuadro.y + (height + spessoreCornice) / 2, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, width, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, -90.0f, Space.Self);
        new_Target.transform.parent = this.transform;


        //angoli
        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x - (width + spessoreCornice) / 2, posizioneQuadro.y + (height + spessoreCornice) / 2, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x + (width + spessoreCornice) / 2, posizioneQuadro.y + (height + spessoreCornice) / 2, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 270.0f, Space.Self);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x + (width + spessoreCornice) / 2, posizioneQuadro.y - (height + spessoreCornice) / 2, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x - (width + spessoreCornice) / 2, posizioneQuadro.y - (height + spessoreCornice) / 2, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
        new_Target.transform.parent = this.transform;

    }
}
