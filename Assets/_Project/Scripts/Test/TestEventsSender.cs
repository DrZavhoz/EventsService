using System.Collections;
using UnityEngine;

namespace FunnyBlox.Events
{
    public class TestEventsSender : MonoBehaviour
    {
        [SerializeField] private EventsService eventsService;

        private void Start()
        {
            StartCoroutine(SenderRoutine());
        }

        private IEnumerator SenderRoutine()
        {
            yield return new WaitForSeconds(2f);
            
            while (true)
            {
                eventsService.TrackEvent($"type{Random.Range(0, 999)}", $"data{Random.Range(0, 999)}");
                
                yield return new WaitForSeconds(Random.Range(0.1f, 1f));
            }
        }
    }
}