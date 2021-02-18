using UnityEngine;
using System.IO;
using System.Collections;

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
    public Material[] mats;
    public string nomeQuadro;
    public int indiceOpera;
    private static int indice;

    public float InstantiateImage(int img, float img_width)
    {
        indiceOpera = indice%mats.Length;
        indice++;
        float img_true_width = img_width;

        //indiceOpera = Random.Range(0, mats.Length);
        Material mat = mats[indiceOpera];
        nomeQuadro = mat.name;
        
        if (img_true_width == 0)
        {
            img_true_width = Random.Range(minSize, maxSize);
        }

        Texture texture = mat.GetTexture("_MainTex");

        float w = texture.width;
        float h = texture.height;
        float img_height = img_true_width * (h / w);
        
        if (img_height > maxHeight)
        {
            img_height = maxHeight;
            img_true_width = img_height * (w / h);
        }

        this.gameObject.GetComponentInChildren<Renderer>().material = mat;
        this.gameObject.transform.localScale = new Vector3(img_true_width, img_height, 1);

        StartCoroutine(AggiungiCornice(img_true_width, img_height));

        return img_true_width;
    }

    private IEnumerator AggiungiCornice(float width, float height)
    {
        GameObject new_Target;
        Vector3 posizioneQuadro = this.gameObject.transform.position;
        //lati sinistro e destro
        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x - (width + spessoreCornice) / 2f, posizioneQuadro.y, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, height, 1);
        new_Target.transform.parent = this.gameObject.transform;

        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x + (width + spessoreCornice) / 2f,  posizioneQuadro.y, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, height, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        new_Target.transform.parent = this.gameObject.transform;

        //lati superiore e inferiore 
        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x, posizioneQuadro.y - (height + spessoreCornice) / 2f, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, width, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
        new_Target.transform.parent = this.gameObject.transform;

        new_Target = Instantiate(latoQuadro, new Vector3(posizioneQuadro.x, posizioneQuadro.y + (height + spessoreCornice) / 2f, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, width, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, -90.0f, Space.Self);
        new_Target.transform.parent = this.gameObject.transform;


        //angoli
        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x - (width + spessoreCornice) / 2f, posizioneQuadro.y + (height + spessoreCornice) / 2f, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x + (width + spessoreCornice) / 2f, posizioneQuadro.y + (height + spessoreCornice) / 2f, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 270.0f, Space.Self);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x + (width + spessoreCornice) / 2f, posizioneQuadro.y - (height + spessoreCornice) / 2f, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        new_Target.transform.parent = this.transform;

        new_Target = Instantiate(angoloQuadro, new Vector3(posizioneQuadro.x - (width + spessoreCornice) / 2f, posizioneQuadro.y - (height + spessoreCornice) / 2f, posizioneQuadro.z + offsetCornice), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
        new_Target.transform.parent = this.transform;

        yield return new WaitForSeconds(0);
    }
    
}