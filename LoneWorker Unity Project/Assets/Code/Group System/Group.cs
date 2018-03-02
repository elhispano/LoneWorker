using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class Group : MonoBehaviour
{
    public int numberOfMembers;

    public float instanceDelay = 0f;

    public IList<Vector3> positions = new List<Vector3>();
    public IList<Vector3> directions = new List<Vector3>();

    public GroupMember memberPrefab;

    public AbstractPath firstPath;

    public float rotationSpeed;

    public bool lookTowardsMovementDirection;

    private GroupFormation currentFormation;

    private IList<GroupMember> members = new List<GroupMember>(); 

    private Quaternion groupRotation = Quaternion.identity;

    private int membersLastIdx = 0;

    private bool isQuitting = false;

    void Awake()
    {
        SetFormation(GetComponent<GroupFormation>());

        if (instanceDelay != 0f)
        {
            StartCoroutine(InstantiateMembersCoroutine());
        }
        else
        {
            for (int i = 0; i < numberOfMembers; i++)
            {
                var newMember = Instantiate<GroupMember>(memberPrefab);
                AddMember(newMember);
            }
        }
    }

    IEnumerator InstantiateMembersCoroutine()
    {
        for (int i = 0; i < numberOfMembers; i++)
        {
            var newMember = Instantiate<GroupMember>(memberPrefab);
            AddMember(newMember);
            yield return new WaitForSeconds(instanceDelay);
        }
    }

    void SetFormation(GroupFormation _newFormation)
    {
        currentFormation = _newFormation;
    }

    public void ComputePositions()
    {
        positions.Clear();
        directions.Clear();

        if (!HasFormation())
            return;

        Vector3 right       = transform.right;
        Vector3 forward     = transform.forward;
        Vector3 up          = transform.up;
        Vector3 position    = transform.position;

        for (int i = 0; i < members.Count; i++)
        {
            var member              = members [i];

            if (member.IsNewPath(firstPath))
            {
                member.ResetProgress();
            }

            float currentProgress   = member.GetCurrentPathProgress();
            Vector3 pathPosition    = firstPath.GetPoint(currentProgress);
            Vector3 pathDirection   = firstPath.GetDirection(currentProgress);

            right   = Vector3.Cross(Vector3.up,pathDirection);
            up      = Vector3.Cross(pathDirection,right.normalized);
            forward = pathDirection;

            Vector3 formationTranslation = currentFormation.GetMemberPosition(member.Index,forward,right,up);

            groupRotation *= Quaternion.AngleAxis(Time.deltaTime * rotationSpeed, pathDirection);
            formationTranslation = groupRotation * formationTranslation;

            Vector3 finalPosition = pathPosition + formationTranslation;

            Debug.DrawRay(finalPosition, right * 2f, Color.green);
            Debug.DrawRay(finalPosition, up * 2f, Color.red);
            Debug.DrawRay(finalPosition, forward * 2f, Color.blue);

            member.IncreasePathProgress(firstPath);
            positions.Add(finalPosition);
            directions.Add(forward);
        }
    }

    public void ApplyAllPositions()
    {
        Vector3 lookToDirection = Vector3.zero;
        for(int i = 0; i < positions.Count; i++)
        {
            if (i >= membersLastIdx)
                break;

            if (!lookTowardsMovementDirection)
                lookToDirection = directions [i];
            else
                lookToDirection = positions[i] - members [i].transform.position;
                    
            members [i].Move(positions [i],lookToDirection);
        }
    }

    void Update()
    {
        ComputePositions();

        if (Input.GetKeyUp(KeyCode.R))
        {
            int randomIndex = UnityEngine.Random.Range(0, members.Count);

            members [randomIndex].gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        ApplyAllPositions();
    }

    bool HasFormation()
    {
        return currentFormation != null;
    }

    public void AddMember(GroupMember _member)
    {
        _member.Initialize(membersLastIdx);
        members.Add(_member);
        membersLastIdx++;
        _member.OnKilled += MemberKilledHandler;
    }

    public void RemoveMember(GroupMember _member)
    {
        int index = _member.Index;

        float previousProgress = members[index].GetCurrentPathProgress();
        for (int i = index+1; i < membersLastIdx; i++)
        {
            float currentProgress = members [i].GetCurrentPathProgress();
            members [i].SetProgress(previousProgress);
            previousProgress = currentProgress;

            members [i].ModifyIndex(-1);
        }

        members.RemoveAt(index);
        membersLastIdx--;
        _member.OnKilled -= MemberKilledHandler;
    }

    void MemberKilledHandler (GroupMember _member)
    {
        if(!isQuitting)
            RemoveMember(_member);
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
