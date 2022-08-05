//------------------------------------------------------------------------------
// DisableCollider.cs
// Created by CYM on 2022/6/2
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class DisableCollider : MonoBehaviour
    {
        [SerializeField]
        bool Is2D = false;

        private void Start()
        {
            if (Is2D)
            {
                Collider2D collider = gameObject.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
            else
            {
                Collider collider = gameObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
    }
}