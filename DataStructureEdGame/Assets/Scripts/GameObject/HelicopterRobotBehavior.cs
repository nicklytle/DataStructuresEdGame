﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * The special helicopter link block
 */
public class HelicopterRobotBehavior : MonoBehaviour, Loggable, LinkContainerEntity
{
    public string logId;

    public GameController gameController;
    public LinkBlockBehavior childLink;
    public Vector3 targetLocation;
    public float flySpeed;

	void Start () {
        childLink.isHelicopterLink = true;
        childLink.gameController = gameController;
    }

    // Update is called once per frame
    void Update() {
        float distAway = Vector3.Distance(transform.position, targetLocation);
        if (distAway > 0.1f)
        {
            Vector3 moveDir = (targetLocation - transform.position).normalized;
            // move, lerping based on the distance away
            transform.Translate(Vector3.Lerp(new Vector3(), (moveDir * flySpeed * Time.deltaTime), distAway / 12f));
            if (childLink != null && childLink.GetComponent<LinkBlockBehavior>().connectingEntity != null)
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
        if (childLink.GetComponent<LinkBlockBehavior>().connectingEntity != null)
        {
            // move above the platform 
            targetLocation = ((MonoBehaviour)childLink.GetComponent<LinkBlockBehavior>().connectingEntity).transform.position + (new Vector3(0, 3, 0));
        }
    }

    public string getLogID()
    {
        return logId;
    }

    public LinkBlockBehavior getChildLink()
    {
        return childLink;
    }

    public void setChildLink(LinkBlockBehavior lb)
    {
        childLink = lb;
    }

    public void updateRenderAndState()
    {
        // none needed.
    }

    public bool isPhasedOut()
    {
        return false;
    }

    public bool isHidden()
    {
        return false;
    }
}
