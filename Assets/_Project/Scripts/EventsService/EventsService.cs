using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace FunnyBlox.Events
{
    public class EventsService : MonoBehaviour
    {
        private const string PLAYERPREFS_EVENTSCOLLECTEDDATA = "EventsCollectedData";
        private const string PLAYERPREFS_EVENTSSENTDATA = "EventsSentData";

        [SerializeField] private string serverUrl = "EMPTY";

        [Space] [SerializeField] private float cooldownBeforeSend = 3f;

        private bool isCooldown;

        private List<SEvent> eventsCollectedList;

        private string eventsSentData;

        private string maskEventsData = "{{\"events\": {0} }}";

        void Start()
        {
            eventsCollectedList = new List<SEvent>();
            isCooldown = false;

            if (PlayerPrefs.HasKey(PLAYERPREFS_EVENTSCOLLECTEDDATA))
            {
                eventsCollectedList =
                    DataFromJson<List<SEvent>>(PlayerPrefs.GetString(PLAYERPREFS_EVENTSCOLLECTEDDATA));
            }
            
            if (PlayerPrefs.HasKey(PLAYERPREFS_EVENTSSENTDATA))
            {
                isCooldown = true;
                StartCoroutine(SendEventsRoutine(PlayerPrefs.GetString(PLAYERPREFS_EVENTSSENTDATA)));
            }
        }

        /// <summary>
        /// Collect events to list
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public void TrackEvent(string type, string data)
        {
            eventsCollectedList.Add(new SEvent { type = type, data = data });

            PlayerPrefs.SetString(PLAYERPREFS_EVENTSCOLLECTEDDATA, DataToJson(eventsCollectedList));

            if (!isCooldown) 
                StartCoroutine(CooldownRoutine());
        }

        /// <summary>
        /// Cooldown coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator CooldownRoutine()
        {
            isCooldown = true;
            yield return new WaitForSeconds(cooldownBeforeSend);

            eventsSentData = PlayerPrefs.GetString(PLAYERPREFS_EVENTSCOLLECTEDDATA);
            PlayerPrefs.SetString(PLAYERPREFS_EVENTSSENTDATA, eventsSentData);

            eventsCollectedList.Clear();
            PlayerPrefs.DeleteKey(PLAYERPREFS_EVENTSCOLLECTEDDATA);

            StartCoroutine(SendEventsRoutine(eventsSentData));
        }

        /// <summary>
        /// Sending events to server
        /// </summary>
        /// <returns></returns>
        private IEnumerator SendEventsRoutine(string eventsData)
        {
            Debug.Log(string.Format(maskEventsData,eventsData));
            
            using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, string.Format(maskEventsData,eventsData)))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (request.responseCode == 200)
                    {
                        Debug.Log("Sending data to server completed");

                        PlayerPrefs.DeleteKey(PLAYERPREFS_EVENTSSENTDATA);

                        isCooldown = false;
                    }
                    else
                    {
                        Debug.LogError("Sending data to server not complete. ResponseCode: "
                                       + request.responseCode);

                        StartCoroutine(SendEventsRoutine(eventsData));
                    }
                }
                else
                {
                    Debug.LogError("Sending data to server not complete. Error:" + request.error);

                    StartCoroutine(SendEventsRoutine(eventsData));
                }
            }
        }

        private string DataToJson<T>(T data) => JsonConvert.SerializeObject(data);

        private T DataFromJson<T>(string data) => JsonConvert.DeserializeObject<T>(data);
    }
}