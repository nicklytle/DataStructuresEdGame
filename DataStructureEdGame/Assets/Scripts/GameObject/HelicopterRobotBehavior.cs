using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterRobotBehavior : MonoBehaviour {

    public string logId;

    public GameController gameController;
    public GameObject childLink;
    public Vector3 targetLocation;
    public float flySpeed;

	void Start () {
        childLink = transform.Find("LinkBlock").gameObject;
        childLink.GetComponent<LinkBlockBehavior>().isHelicopterLink = true;
        childLink.GetComponent<LinkBlockBehavior>().gameController = gameController;
    }

    // Update is called once per frame
    void Update() {
        float distAway = Vector3.Distance(transform.position, targetLocation);
        if (distAway > 0.1f)
        {
            Vector3 moveDir = (targetLocation - transform.position).normalized;
            // move, lerping based on the distance away
            transform.Translate(Vector3.Lerp(new Vector3(), (moveDir * flySpeed * Time.deltaTime), distAway / 12f));
            if (childLink != null && childLink.GetComponent<LinkBlockBehavior>().connectingPlatform != null)
            {
                childLink.GetComponent<LinkBlockBehavior>().setRenderArrow(false);
                // childLink.GetComponent<LinkBlockBehavior>().UpdateLinkArrow(); // refresh its position as it moves
            }
        } else
        {
            childLink.GetComponent<LinkBlockBehavior>().setRenderArrow(true);
        }
    }

    public void MoveAboveLinkedPlatform()
    {
        if (childLink.GetComponent<LinkBlockBehavior>().connectingPlatform != null)
        {
            // move above the platform 
            targetLocation = childLink.GetComponent<LinkBlockBehavior>().connectingPlatform.transform.position + (new Vector3(0, 3, 0));
        }
    }

    public string getLogID()
    {
        return logId;
    }
}
