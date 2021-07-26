using System.Collections;
using UnityEngine;

namespace Utils.Cameras
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] Vector3 starPosition;
        [SerializeField] Vector3 endPosition;
        [SerializeField] float time;
        [SerializeField] private bool startMoving = true;
        private float initialTime = float.MinValue;


        private void Awake()
        {
            if (startMoving)
            {
                StartCoroutine(Move(5));
            }
        }


        void Update()
        {
            if (Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.R))
            {
                StopAllCoroutines();
                StartCoroutine(Move());
            }
        }


        IEnumerator Move(float waitForSeconds = 1)
        {
            transform.position = starPosition;

            yield return new WaitForSeconds(waitForSeconds);

            initialTime = Time.time;

            float t = 0;

            while (t < 1)
            {
                t = (Time.time - initialTime) / time;

                transform.position = Vector3.Lerp(starPosition, endPosition, t);
                transform.LookAt(endPosition);

                yield return null;
            }

            yield return null;
        }

    }
}