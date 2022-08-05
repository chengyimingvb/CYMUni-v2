using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CYM
{
    public class CreditsTool : EditorWindow
    {
        #region Variables
        public Vector2 scrollPosition;
        public TextAsset textFileCredits;
        public GameObject scrollViewContent;
        public string[] optionsItemType = new string[] { "Title", "Name", "Info" };
        public int indexItemType;
        public string[] optionsColorItem = new string[] { "Color Titles", "Color Names", "Color Info" };
        public int indexColorItem;
        private string itemName = null;
        private string urlAddress = "http://www.baidu.com/";
        private string urlButtonText;
        private string tempItemName;
        private string tempURL;
        private string tempButtonText;
        private Sprite imageElement;
        private Color itemColor = new Color(0.5f, 0.5f, 0.5f, 1);
        private Font fontElement;
        #endregion

        void Awake()
        {
        }
        // Add menu named "My Window" to the Window menu
        [MenuItem("Tools/Windows/Credits")]
        static void Init()
        {
            CreditsTool window = (CreditsTool)EditorWindow.GetWindow(typeof(CreditsTool));
            GUIContent titleContent = new GUIContent("Credits");
            window.titleContent = titleContent;
            window.minSize = new Vector2(100, 250);
            window.Show();
        }

        void OnGUI()
        {
            #region Enter Pressed
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                if (GUI.GetNameOfFocusedControl() == "namingField")
                {
                    if (scrollViewContent != null)
                    {
                        if (tempItemName != null)
                        {
                            tempItemName = null;
                            Repaint();
                            EditorGUI.FocusTextInControl("namingField");
                            PlaceCreditItem();
                        }
                    }
                    else
                    {
                        //EditorGUI.FocusTextInControl("contentField");
                        Debug.Log("Must assign the scroll view content before placing elements.");
                    }
                }
                else if (GUI.GetNameOfFocusedControl() == "urlField" || GUI.GetNameOfFocusedControl() == "buttonTextField")
                {
                    if (scrollViewContent != null)
                    {
                        if (tempURL == "")
                            tempURL = null;
                        if (tempButtonText == "")
                            tempButtonText = null;

                        if (tempURL != null && tempButtonText != null)
                        {
                            tempButtonText = null;
                            tempURL = null;
                            GUI.FocusControl("Clear");
                            Repaint();
                            PlaceURLButton();
                        }
                        else if (tempURL != null && tempButtonText == null)
                        {
                            EditorGUI.FocusTextInControl("buttonTextField");
                            Repaint();
                            Debug.Log("Must fill in Button text field.");
                        }
                        else if (tempURL == null && tempButtonText != null)
                        {
                            EditorGUI.FocusTextInControl("urlField");
                            Repaint();
                            Debug.Log("Must fill in URL text field.");
                        }
                        else
                        {
                            Debug.Log("Must fill in URL and Button text fields.");
                        }
                    }
                    else
                    {
                        //EditorGUI.FocusTextInControl("contentField");
                        Debug.Log("Must assign the scroll view content before placing elements.");
                    }
                }
            }
            #endregion

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));
            GUILayout.BeginVertical();

            #region ScrollView Content
            GUILayout.Label("ScrollView Content:",PreviewUtil.Head);
            GameObject tempHas = scrollViewContent;
            scrollViewContent = EditorGUILayout.ObjectField(scrollViewContent, typeof(GameObject), true) as GameObject;
            if (scrollViewContent != tempHas)
            {
                if (scrollViewContent != null)
                {
                    VerticalLayoutGroup componentA = scrollViewContent.GetComponent<VerticalLayoutGroup>();
                    ContentSizeFitter componentB = scrollViewContent.GetComponent<ContentSizeFitter>();
                    if (componentA == null || componentB == null)
                    {
                        if (componentA == null)
                        {
                            VerticalLayoutGroup vlg = scrollViewContent.AddComponent<VerticalLayoutGroup>();
                            vlg.spacing = 4;
                            vlg.childAlignment = TextAnchor.UpperCenter;
                            vlg.childForceExpandHeight = false;
                            vlg.childForceExpandWidth = false;
                        }
                        if (componentB == null)
                        {
                            ContentSizeFitter csf = scrollViewContent.AddComponent<ContentSizeFitter>();
                            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        }
                    }
                }
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            #region Text Element
            GUILayout.Space(20);
            GUILayout.Label("Text Element:", PreviewUtil.Head);
            indexItemType = EditorGUILayout.Popup(indexItemType, optionsItemType);
            GUILayout.Label("Field Name:");
            tempItemName = itemName;
            itemName = EditorGUILayout.TextField(itemName);
            if (GUILayout.Button("Insert Item", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                if (tempItemName != null)
                {
                    tempItemName = null;
                    GUI.FocusControl("Clear");
                    PlaceCreditItem();
                }
                else
                {
                    Debug.Log("Must fill in name field.");
                }
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            #region Item
            GUILayout.Space(20);
            GUILayout.Label("Misc Element:", PreviewUtil.Head);
            if (GUILayout.Button("Line Break", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                GUI.FocusControl("Clear");
                PlaceBlankItem();
            }
            if (GUILayout.Button("Divider", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                GUI.FocusControl("Clear");
                PlaceDividerItem();
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            #region Internet Address
            GUILayout.Space(20);
            GUILayout.Label("Internet Address:", PreviewUtil.Head);
            tempURL = urlAddress;
            urlAddress = EditorGUILayout.TextField(urlAddress);
            GUILayout.Label("Button Text:");
            tempButtonText = urlButtonText;
            urlButtonText = EditorGUILayout.TextField(urlButtonText);
            if (GUILayout.Button("URL Button", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                if (scrollViewContent != null)
                {
                    if (tempURL == "")
                        tempURL = null;
                    if (tempButtonText == "")
                        tempButtonText = null;

                    if (tempURL != null && tempButtonText != null)
                    {
                        tempButtonText = null;
                        tempURL = null;
                        GUI.FocusControl("Clear");
                        Repaint();
                        PlaceURLButton();
                    }
                    else if (tempURL != null && tempButtonText == null)
                    {
                        EditorGUI.FocusTextInControl("buttonTextField");
                        Repaint();
                        Debug.Log("Must fill in Button text field.");
                    }
                    else if (tempURL == null && tempButtonText != null)
                    {
                        EditorGUI.FocusTextInControl("urlField");
                        Repaint();
                        Debug.Log("Must fill in URL text field.");
                    }
                    else
                    {
                        Debug.Log("Must fill in URL and Button text fields.");
                    }
                }
                else
                {
                    EditorGUI.FocusTextInControl("contentField");
                    Debug.Log("Must assign the scroll view content before placing elements.");
                }
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            #region Source image
            GUILayout.Space(20);
            GUILayout.Label("Source Image:", PreviewUtil.Head);
            imageElement = EditorGUILayout.ObjectField(imageElement, typeof(Sprite), true) as Sprite;
            if (GUILayout.Button("Insert Image", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                if (imageElement != null)
                {
                    GUI.FocusControl("Clear");
                    PlaceImage();
                }
                else
                {
                    EditorGUI.FocusTextInControl("spriteField");
                    Debug.Log("Must define sprite first.");
                }
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            #region Mass Color
            GUILayout.Space(20);
            GUILayout.Label("Mass Color:", PreviewUtil.Head);
            indexColorItem = EditorGUILayout.Popup(indexColorItem, optionsColorItem);
            itemColor = EditorGUILayout.ColorField(itemColor);
            if (GUILayout.Button("Colorize", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                ColorizeItems();
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            #region Font
            GUILayout.Space(20);
            GUILayout.Label("Font:", PreviewUtil.Head);
            fontElement = EditorGUILayout.ObjectField(fontElement, typeof(Font), true) as Font;
            if (GUILayout.Button("Change Font", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                if (fontElement != null)
                {
                    GUI.FocusControl("Clear");
                    ChangeFontAll();
                }
                else
                {
                    EditorGUI.FocusTextInControl("fontField");
                    Debug.Log("Must choose desired font.");
                }
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            #region Import Text File
            GUILayout.Space(20);
            GUILayout.Label("Import Text File:", PreviewUtil.Head);
            textFileCredits = EditorGUILayout.ObjectField(textFileCredits, typeof(TextAsset), true) as TextAsset;
            if (GUILayout.Button("Import", GUILayout.MinWidth(55), GUILayout.MinHeight(30)))
            {
                GUI.FocusControl("Clear");
                ProcessTextFile();
            }
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(4));
            #endregion

            GUILayout.Space(100);
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void ProcessTextFile()
        {
            if (textFileCredits == null)
            {
                EditorGUI.FocusTextInControl("textFileField");
                Debug.Log("You haven't chosen a text file.");
            }
            if (scrollViewContent == null)
            {
                EditorGUI.FocusTextInControl("contentField");
                Debug.Log("You haven't specidifed the scroll view content object.");
            }
            else if (textFileCredits != null && scrollViewContent != null)
            {
                var textArray = textFileCredits.text.Split('\n');
                foreach (string credit in textArray)
                {
                    if (credit.Contains("<t>"))
                    {
                        GameObject creditItemTitle = Instantiate(Resources.Load("Prefabs/RichText/Title", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemTitle, "Created Credits");
                        creditItemTitle.transform.SetParent(scrollViewContent.transform);
                        creditItemTitle.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosTitle = creditItemTitle.transform.localPosition;
                        localPosTitle.z = 0;
                        creditItemTitle.transform.localPosition = localPosTitle;
                        itemName = credit.Replace("<t>", "");
                        creditItemTitle.name = "<T> " + itemName;
                        Text textComponentTitle = creditItemTitle.GetComponent<Text>();
                        textComponentTitle.text = itemName;
                    }
                    else if (credit.Contains("<n>"))
                    {
                        GameObject creditItemName = Instantiate(Resources.Load("Prefabs/RichText/Name", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemName, "Created Credits");
                        creditItemName.transform.SetParent(scrollViewContent.transform);
                        creditItemName.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosName = creditItemName.transform.localPosition;
                        localPosName.z = 0;
                        creditItemName.transform.localPosition = localPosName;
                        itemName = credit.Replace("<n>", "");
                        creditItemName.name = "<N> " + itemName;
                        Text textComponentName = creditItemName.GetComponent<Text>();
                        textComponentName.text = itemName;
                    }
                    else if (credit.Contains("<br>"))
                    {
                        GameObject creditItemSpacer = Instantiate(Resources.Load("Prefabs/RichText/Spacer", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemSpacer, "Created Credits");
                        creditItemSpacer.transform.SetParent(scrollViewContent.transform);
                        creditItemSpacer.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosSpacer = creditItemSpacer.transform.localPosition;
                        localPosSpacer.z = 0;
                        creditItemSpacer.transform.localPosition = localPosSpacer;
                        creditItemSpacer.name = "<br>";
                    }
                    else if (credit.Contains("<i>"))
                    {
                        GameObject creditItemDescription = Instantiate(Resources.Load("Prefabs/RichText/Information", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemDescription, "Created Credits");
                        creditItemDescription.transform.SetParent(scrollViewContent.transform);
                        creditItemDescription.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosDescription = creditItemDescription.transform.localPosition;
                        localPosDescription.z = 0;
                        creditItemDescription.transform.localPosition = localPosDescription;
                        itemName = credit.Replace("<i>", "");
                        Text textComponentDescription = creditItemDescription.GetComponent<Text>();
                        textComponentDescription.text = itemName;
                        if (itemName.Length >= 10)
                            itemName = itemName.Substring(0, 10) + "...";
                        creditItemDescription.name = "<I> " + itemName;
                    }
                    else if (credit.Contains("<divider>"))
                    {
                        GameObject creditItemSeparator = Instantiate(Resources.Load("Prefabs/RichText/Divider", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemSeparator, "Created Credits");
                        creditItemSeparator.transform.SetParent(scrollViewContent.transform);
                        creditItemSeparator.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosSeparator = creditItemSeparator.transform.localPosition;
                        localPosSeparator.z = 0;
                        creditItemSeparator.transform.localPosition = localPosSeparator;
                        creditItemSeparator.name = "<divider>";
                    }
                }
                itemName = null;
            }
        }
        private void PlaceCreditItem()
        {
            if (scrollViewContent != null)
            {
                //Places a menu prefab item depending on selection type.
                switch (indexItemType)
                {
                    case 0:
                        GameObject creditItemTitle = Instantiate(Resources.Load("Prefabs/RichText/Title", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemTitle, "Created Credits");
                        creditItemTitle.transform.SetParent(scrollViewContent.transform);
                        creditItemTitle.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosTitle = creditItemTitle.transform.localPosition;
                        localPosTitle.z = 0;
                        creditItemTitle.transform.localPosition = localPosTitle;
                        creditItemTitle.name = "<T> " + itemName;
                        Text textComponentTitle = creditItemTitle.GetComponent<Text>();
                        textComponentTitle.text = itemName;
                        Selection.activeGameObject = creditItemTitle;
                        itemName = null;
                        break;
                    case 1:
                        GameObject creditItemName = Instantiate(Resources.Load("Prefabs/RichText/Name", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemName, "Created Credits");
                        creditItemName.transform.SetParent(scrollViewContent.transform);
                        creditItemName.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosName = creditItemName.transform.localPosition;
                        localPosName.z = 0;
                        creditItemName.transform.localPosition = localPosName;
                        creditItemName.name = "<N> " + itemName;
                        Text textComponentName = creditItemName.GetComponent<Text>();
                        textComponentName.text = itemName;
                        Selection.activeGameObject = creditItemName;
                        itemName = null;
                        break;
                    case 2:
                        GameObject creditItemDescription = Instantiate(Resources.Load("Prefabs/RichText/Information", typeof(GameObject))) as GameObject;
                        Undo.RegisterCreatedObjectUndo(creditItemDescription, "Created Credits");
                        creditItemDescription.transform.SetParent(scrollViewContent.transform);
                        creditItemDescription.transform.localScale = new Vector3(1, 1, 1);
                        Vector3 localPosDescription = creditItemDescription.transform.localPosition;
                        localPosDescription.z = 0;
                        creditItemDescription.transform.localPosition = localPosDescription;
                        Text textComponentDescription = creditItemDescription.GetComponent<Text>();
                        textComponentDescription.text = itemName;
                        if (itemName.Length >= 10)
                            itemName = itemName.Substring(0, 10) + "...";
                        creditItemDescription.name = "<I> " + itemName;
                        Selection.activeGameObject = creditItemDescription;
                        itemName = null;
                        break;
                }
            }
            else
            {
                EditorGUI.FocusTextInControl("contentField");
                Debug.Log("Must assign the scroll view content before placing elements.");
            }
        }
        private void PlaceBlankItem()
        {
            if (scrollViewContent != null)
            {
                GameObject creditItemSpacer = Instantiate(Resources.Load("Prefabs/RichText/Spacer", typeof(GameObject))) as GameObject;
                Undo.RegisterCreatedObjectUndo(creditItemSpacer, "Created Credits");
                creditItemSpacer.transform.SetParent(scrollViewContent.transform);
                creditItemSpacer.transform.localScale = new Vector3(1, 1, 1);
                Vector3 localPosSpacer = creditItemSpacer.transform.localPosition;
                localPosSpacer.z = 0;
                creditItemSpacer.transform.localPosition = localPosSpacer;
                creditItemSpacer.name = "<br>";
                Selection.activeGameObject = creditItemSpacer;
            }
            else
            {
                EditorGUI.FocusTextInControl("contentField");
                Debug.Log("Must assign the scroll view content before placing elements.");
            }
        }

        private void PlaceDividerItem()
        {
            if (scrollViewContent != null)
            {
                GameObject creditItemSeparator = Instantiate(Resources.Load("Prefabs/RichText/Divider", typeof(GameObject))) as GameObject;
                Undo.RegisterCreatedObjectUndo(creditItemSeparator, "Created Credits");
                creditItemSeparator.transform.SetParent(scrollViewContent.transform);
                creditItemSeparator.transform.localScale = new Vector3(1, 1, 1);
                Vector3 localPosSeparator = creditItemSeparator.transform.localPosition;
                localPosSeparator.z = 0;
                creditItemSeparator.transform.localPosition = localPosSeparator;
                creditItemSeparator.name = "<divider>";
                Selection.activeGameObject = creditItemSeparator;
            }
            else
            {
                EditorGUI.FocusTextInControl("contentField");
                Debug.Log("Must assign the scroll view content before placing elements.");
            }
        }

        private void PlaceURLButton()
        {
            GameObject urlButton = Instantiate(Resources.Load("Prefabs/RichText/URL", typeof(GameObject))) as GameObject;
            Undo.RegisterCreatedObjectUndo(urlButton, "Created Credits");
            urlButton.transform.SetParent(scrollViewContent.transform);
            urlButton.transform.localScale = new Vector3(1, 1, 1);
            Vector3 localButtonSpacer = urlButton.transform.localPosition;
            localButtonSpacer.z = 0;
            urlButton.transform.localPosition = localButtonSpacer;
            urlButton.name = "<URL> " + urlAddress;

            //Sets button text.
            Text textCompUrlButton = urlButton.GetComponentInChildren<Text>();
            textCompUrlButton.text = urlButtonText;

            //Sets URL in script component.
            CYM.ButtonOpenURL OpenURLScript = urlButton.GetComponent<ButtonOpenURL>();
            OpenURLScript.url = urlAddress;

            Selection.activeGameObject = urlButton;

            urlAddress = null;
            urlButtonText = null;
        }

        private void PlaceImage()
        {
            if (scrollViewContent != null)
            {
                GameObject creditsImage = Instantiate(Resources.Load("Prefabs/RichText/Image", typeof(GameObject))) as GameObject;
                Undo.RegisterCreatedObjectUndo(creditsImage, "Created Credits");
                creditsImage.transform.SetParent(scrollViewContent.transform);
                creditsImage.transform.localScale = new Vector3(1, 1, 1);
                Vector3 localImageSpacer = creditsImage.transform.localPosition;
                localImageSpacer.z = 0;
                creditsImage.transform.localPosition = localImageSpacer;
                creditsImage.name = "<IMG> " + imageElement.name;
                Image imgComponent = creditsImage.GetComponent<Image>();
                imgComponent.sprite = imageElement;
                Selection.activeGameObject = creditsImage;
            }
            else
            {
                EditorGUI.FocusTextInControl("contentField");
                Debug.Log("Must assign the scroll view content before placing elements.");
            }
        }

        private void ColorizeItems()
        {
            if (scrollViewContent != null)
            {
                //Gets list of all children in the scroll view content.
                var creditsList = new List<GameObject>();
                foreach (Transform child in scrollViewContent.transform)
                {
                    creditsList.Add(child.gameObject);
                }

                //Colors credit items depending on selection type.
                switch (indexColorItem)
                {
                    case 0:
                        foreach (GameObject element in creditsList)
                        {
                            var goName = element.name;
                            if (goName.Contains("<T>"))
                            {
                                Text textComponent = element.GetComponent<Text>();
                                textComponent.color = itemColor;
                            }
                        }
                        break;
                    case 1:
                        foreach (GameObject element in creditsList)
                        {
                            var goName = element.name;
                            if (goName.Contains("<N>"))
                            {
                                Text textComponent = element.GetComponent<Text>();
                                textComponent.color = itemColor;
                            }
                        }
                        break;
                    case 2:
                        foreach (GameObject element in creditsList)
                        {
                            var goName = element.name;
                            if (goName.Contains("<I>"))
                            {
                                Text textComponent = element.GetComponent<Text>();
                                textComponent.color = itemColor;
                            }
                        }
                        break;
                }
                //This simply refreshes the scene and colors by creating and destroying an empty game object.
                var go = new GameObject();
                DestroyImmediate(go);
            }
            else
            {
                EditorGUI.FocusTextInControl("contentField");
                Debug.Log("Must assign the scroll view content before colorizing elements.");
            }
        }

        private void ChangeFontAll()
        {
            if (scrollViewContent != null)
            {
                //Gets list of all children in the scroll view content.
                foreach (Transform child in scrollViewContent.transform)
                {
                    if (child.GetComponent<Text>() != null)
                    {
                        Text textComponent = child.GetComponent<Text>();
                        textComponent.font = fontElement;
                    }
                }
                //This simply refreshes the scene and colors by creating and destroying an empty game object.
                var go = new GameObject();
                DestroyImmediate(go);
            }
            else
            {
                EditorGUI.FocusTextInControl("contentField");
                Debug.Log("Must assign the scroll view content before changing font.");
            }
        }
    }
}