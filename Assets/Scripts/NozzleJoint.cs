using UnityEngine;
using UnityEditor;

public class NozzleJoint : MonoBehaviour
{
    // 메뉴 아이콘을 눌러 실행하려면 Editor 폴더에 있거나, 
    // 아래 메서드가 static이어야 합니다.
    [MenuItem("Tools/Add Joints to Children")]
    static void AddJoints() {
        GameObject root = Selection.activeGameObject;
        if (root == null) {
            Debug.LogWarning("루트 오브젝트를 선택해주세요.");
            return;
        }

        // 모든 자식들을 순회 (깊이 우선 탐색)
        foreach (Transform t in root.GetComponentsInChildren<Transform>()) {
            // 1. 루트는 Joint 연결 대상에서 제외 (보통 고정점)
            if (t == root.transform) {
                if (t.GetComponent<Rigidbody>() == null) {
                    Rigidbody rootRb = t.gameObject.AddComponent<Rigidbody>();
                    rootRb.isKinematic = true; // 루트는 보통 고정됨
                }
                continue;
            }

            // 2. CapsuleCollider 추가
            CapsuleCollider col = t.GetComponent<CapsuleCollider>();
            if (col == null)
                col = Undo.AddComponent<CapsuleCollider>(t.gameObject);
            col.radius = 0.005f;
            col.height = 0.01f;

            // 3. Rigidbody 추가 및 설정
            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb == null)
                rb = Undo.AddComponent<Rigidbody>(t.gameObject);
            // 고무호스: 적당한 무게감 + 저항으로 늘어지는 느낌
            rb.mass = 0.15f;
            rb.drag = 3f;
            rb.angularDrag = 4f;

            // 4. ConfigurableJoint 추가 및 부모 연결
            ConfigurableJoint joint = t.GetComponent<ConfigurableJoint>();
            if (joint == null) {
                joint = t.gameObject.AddComponent<ConfigurableJoint>();
            }

            // 부모의 Rigidbody를 찾아 연결
            Rigidbody parentRb = t.parent.GetComponent<Rigidbody>();
            if (parentRb != null) {
                joint.connectedBody = parentRb;
            } else {
                Debug.LogWarning($"{t.name}의 부모에게 Rigidbody가 없어 연결하지 못했습니다.");
            }

            // 위치: 세그먼트가 분리되지 않도록 고정
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            // 각도: Limited + 범위 설정으로 자연스럽게 휘도록
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            // X축 회전 범위 ±85도 (거의 자유 회전 — 세그먼트당 85도씩 쌓이면 돌돌 말림)
            joint.lowAngularXLimit  = new SoftJointLimit { limit = -85f };
            joint.highAngularXLimit = new SoftJointLimit { limit =  85f };

            // Y, Z축도 ±85도
            SoftJointLimit wideLimit = new SoftJointLimit { limit = 85f };
            joint.angularYLimit = wideLimit;
            joint.angularZLimit = wideLimit;

            // 스프링 거의 없음 — 고무처럼 늘어진 채로 유지, 댐퍼로 진동만 잡음
            JointDrive angularDrive = new JointDrive {
                positionSpring = 1f,
                positionDamper = 2f,
                maximumForce = 5f
            };
            joint.angularXDrive  = angularDrive;
            joint.angularYZDrive = angularDrive;
        }
        
        Debug.Log("Joint 및 Rigidbody 일괄 적용 완료!");
    }
}