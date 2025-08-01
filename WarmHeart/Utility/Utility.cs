using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Utility
{
    /// <summary>
    /// center를 중심으로 distance만큼의 범위로
    /// areaMask에 포함되는 random한 좌표를 반환한다.
    /// </summary>
    public static Vector3 GetRandPointOnNavMesh(Vector3 center, float distance, int areaMask)
    {
        var randPos = Random.insideUnitSphere * distance + center;
        randPos.z = 1;

        NavMeshHit hit;
        NavMesh.SamplePosition(randPos, out hit, distance, areaMask);

        return hit.position;
    }

    /// <summary>
    /// 평균(mean)과 표준편차(standard)를 통해
    /// 정규분포 난수를 생성한다.
    /// </summary>
    public static float GetRandNormalDistribution(float mean, float standard)
    {
        var x1 = Random.Range(0f, 1f);
        var x2 = Random.Range(0f, 1f);
        return mean + standard * (Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Sin(2.0f * Mathf.PI * x2));
    }

    /// <summary>
    /// 임의의 확률을 선택한다.
    /// ex) bool epicItem = GCR(0.001) → 1/1000의 확률로 크리티컬이 뜬다.
    /// </summary>
    public static bool GetChanceResult(float chance)
    {
        if (chance < 0.0000001f)
            chance = 0.0000001f;

        bool success = false;
        int randAccuracy = 10000000; // 천만. 천만분의 chance의 확률이다.
        float randHitRange = chance * randAccuracy;

        int rand = Random.Range(1, randAccuracy + 1);
        if (rand <= randHitRange)
            success = true;

        return success;
    }

    /// <summary>
    /// 임의의 퍼센트 확률을 선택한다.
    /// ex) bool critical = GPCR(30) → 30% 확률로 크리티컬이 뜬다.
    /// </summary>
    public static bool GetPercentageChanceResult(float perChance)
    {
        if (perChance < 0.0000001f)
            perChance = 0.0000001f;

        perChance = perChance / 100;

        bool success = false;
        int randAccuracy = 10000000; // 천만. 천만분의 chance의 확률이다.
        float randHitRange = perChance * randAccuracy;

        int rand = Random.Range(1, randAccuracy + 1);
        if (rand <= randHitRange)
            success = true;

        return success;
    }

    /// <summary>
    /// 임의의 타입 T인 List 안의 두 값을 변경한다.
    /// </summary>
    /// <typeparam name="T"> 타입 지정 </typeparam>
    /// <param name="list"> 임의의 타입 List</param>
    /// <param name="from"> 변경하고자 하는 값 A </param>
    /// <param name="to"> A와 변경할 값 B </param>
    public static void SwapListElement<T>(this List<T> list, int from, int to)
    {
        T temp = list[from];
        list[from] = list[to];
        list[to] = temp;
    }

    /// <summary>
    /// 임의의 타입 T인 List 안에서 element의 index를 찾는다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="element"></param>
    /// <returns></returns>
    public static int FindIndexOf<T>(this List<T> list, T element)
    {
        return list.IndexOf(element);
    }

    public static GameObject FindNearestObjectByTag(Transform owner, string tag)
    {
        var objs = GameObject.FindGameObjectsWithTag(tag).ToList();

        var nearestObj = objs.OrderBy(obj =>
        {
            return Vector3.Distance(owner.position, obj.transform.position);
        }).FirstOrDefault();

        return nearestObj;
    }

    public static GameObject FindNearestObjectByTags(Transform owner, string[] tags)
    {
        GameObject nearestObj = null;

        for (int i = 0; i < tags.Length; i++)
        {
            var objs = GameObject.FindGameObjectsWithTag(tags[i]).ToList();

            nearestObj = objs.OrderBy(obj =>
            {
                return Vector3.Distance(owner.position, obj.transform.position);
            }).FirstOrDefault();
        }

        return nearestObj;
    }

    /// <summary>
    /// target이 메인 카메라 안에 있는지 확인한다.
    /// </summary>
    public static bool IsExistObjectInCamera(Transform target)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(target.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1
            && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }
}