using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QuantumTek.QuantumDialogue;

public class MyDialog : MonoBehaviour{
        public QD_DialogueHandler handler;
        public TextMeshProUGUI speakerName;
        public TextMeshProUGUI messageText;
        public Transform choices;
        public TextMeshProUGUI choiceTemplate;
        public string dialogo;

        private bool ended;

        private void Awake(){
            handler.SetConversation(dialogo);
            SetText();
        }

        private void Update(){
            // Don't do anything if the conversation is over
            if (ended){
                gameObject.SetActive(false); //Chapucilla
                return;
            }
            // Check if the space key is pressed and the current message is not a choice
            if (handler.currentMessageInfo.Type == QD_NodeType.Message && Input.GetKeyUp(KeyCode.Return))
                Next();
        }


        private void SetText(){
            // Clear everything
            speakerName.text = "";
            messageText.gameObject.SetActive(false);
            messageText.text = "";

            // If at the end, don't do anything
            if (ended)
                return;

            // Generate choices if a choice, otherwise display the message
            if (handler.currentMessageInfo.Type == QD_NodeType.Message){
                QD_Message message = handler.GetMessage();
                speakerName.text = message.SpeakerName;
                messageText.text = message.MessageText;
                messageText.gameObject.SetActive(true);

            }
            else if (handler.currentMessageInfo.Type == QD_NodeType.Choice)
                speakerName.text = "Player";
        }

        public void Next(int choice = -1){
            if (ended)
                return;
            
            // Go to the next message
            handler.NextMessage(choice);
            // Set the new text
            SetText();
            // End if there is no next message
            if (handler.currentMessageInfo.ID < 0)
                ended = true;
        }
}
