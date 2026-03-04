using UnityEngine;

public class Bobbing3DLetters : MonoBehaviour
{
    public GameObject[] letters;
    public float bobbingAmplitude = 0.5f;
    public float bobbingSpeed = 2f;

    private Vector3[] basePositions;


    private void Start()
    {
        letters = new GameObject[transform.childCount];
        basePositions = new Vector3[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            letters[i] = transform.GetChild(i).gameObject;
            basePositions[i] = letters[i].transform.localPosition;
        }
    }

    private void Update()
    {
        if (letters == null || letters.Length == 0)
        {
            return;
        }

        for (int i = 0; i < letters.Length; i++)
        {
            if (letters[i] == null)
            {
                continue;
            }

            float bobbingOffset = Mathf.Sin(Time.time * bobbingSpeed + i) * bobbingAmplitude;
            Vector3 basePosition = basePositions[i];
            letters[i].transform.localPosition = new Vector3(basePosition.x, basePosition.y + bobbingOffset, basePosition.z);
        }
    }


}
