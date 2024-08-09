using Tools.DataAssets.Nodes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils.Injection;

namespace View
{
    public class DialogueUI : InjectableBehaviour
    {

        public Image left;
        public Transform right;

        public Text prompt;
        public Transform answers;
        public DialogueAnswer answerPrefab;

        public UnityEvent<int> answerSelected;

        public void Setup(Transform recipient)
        {
            var pos = recipient.position;
            right.position = pos + recipient.forward / 2 + Vector3.up * 1.6f;
            right.LookAt(pos + Vector3.up * 1.6f, Vector3.up);

//        StartCoroutine(nameof(LoadAvatar));
        }

        // IEnumerator LoadAvatar()
        // {
        //     UnityWebRequest www = UnityWebRequestTexture.GetTexture("https://" + _avatar.Get().media);
        //     yield return www.SendWebRequest();
        //
        //     if (www.isNetworkError || www.isHttpError)
        //     {
        //         Debug.LogError(www.error);
        //     }
        //     else
        //     {
        //         Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        //         Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2());
        //         left.sprite = sprite;
        //     }
        // }

        public void ShowChat(ChatNode chat)
        {
            prompt.text = chat.prompt;

            foreach (Transform child in answers)
            {
                child.GetComponent<DialogueAnswer>().selected.RemoveAllListeners();
                Destroy(child.gameObject);
            }

            var i = 0;
            foreach (var answer in chat.answer)
            {
                var dialogueAnswer = Instantiate(answerPrefab, answers);
                dialogueAnswer.Setup(i++, answer);
                dialogueAnswer.selected.AddListener(OnAnswerSelected);
            }
        }

        private void OnAnswerSelected(int index)
        {
            answerSelected.Invoke(index);
        }
    }
}