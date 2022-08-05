//------------------------------------------------------------------------------
// FrustumCullObj.cs
// Created by CYM on 2022/3/29
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class FrustumCullObj : MonoBehaviour
    {
        public float disableAngle = 0f;

        Camera mainCam;
        bool turnedOff;
        int frames = 0;
        Renderer objRenderer;

        void Start()
        {
            mainCam = BaseGlobal.MainCamera;

            if (gameObject.GetComponentInChildren<Renderer>())
            {
                objRenderer = gameObject.GetComponentInChildren<Renderer>();
            }
            
        }
        private void OnEnable()
        {
            JoinQueue(gameObject);
        }
        private void OnDisable()
        {
            QuitQueue(gameObject);
        }

        void UpdateRender()
        {
            if (!objRenderer) return;

            frames++;

            //run once every 5 frames
            if (frames >= 5)
            {
                frames = 0;

                gameObject.transform.position = transform.position;
                gameObject.transform.rotation = transform.rotation;

                if (mainCam != null)
                {
                    bool onAngle = false;
                    float dot = 0f;

                    if (disableAngle != 0f)
                    {
                        Vector3 toObject = (transform.position - mainCam.transform.position).normalized;
                        dot = Vector3.Dot(toObject, mainCam.transform.forward);
                        onAngle = dot >= disableAngle;
                    }

                    if (objRenderer.isVisible)
                    {
                        disableAngle = dot;
                    }

                    if (onAngle)
                    {
                        if (turnedOff) EnableObject();
                    }
                    else
                    {
                        if (!turnedOff) DisableObject();
                    }

                }
                else
                {
                    Debug.LogWarning("No game camera set");
                }
            }
        }

        void DisableObject()
        {
            if (objRenderer.isVisible) 
                return;

            Vector3 toObject = (transform.position - mainCam.transform.position).normalized;
            disableAngle = Vector3.Dot(toObject, mainCam.transform.forward);
            objRenderer.enabled = false;
            turnedOff = true;
        }

        void EnableObject()
        {
            Vector3 toObject = (transform.position - mainCam.transform.position).normalized;
            disableAngle = Vector3.Dot(toObject, mainCam.transform.forward);
            objRenderer.enabled = true;
            turnedOff = false;
        }
        string JobUpdateKey = "FrustumCulling";
        int JobsPerFrame = 50;
        void JoinQueue(GameObject mono)
        {
            if (QueueHub.DoesQueueExist(JobUpdateKey))
            {
                QueueHub.AddJobToQueue(JobUpdateKey, mono, UpdateRender);
            }
            else
            {
                QueueHub.CreateQueue(JobUpdateKey, JobsPerFrame, true);
                QueueHub.AddJobToQueue(JobUpdateKey, mono, UpdateRender);
            }
        }
        void QuitQueue(GameObject mono)
        {
            if (QueueHub.DoesQueueExist(JobUpdateKey))
            {
                QueueHub.RemoveJobFromQueue(JobUpdateKey, mono);
            }
            else
            {
                QueueHub.CreateQueue(JobUpdateKey, JobsPerFrame, true);
            }
        }
    }
}