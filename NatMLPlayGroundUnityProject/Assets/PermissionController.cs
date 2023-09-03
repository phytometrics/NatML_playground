using UnityEngine;
using System.Collections;
using System;

// ソースコードはすべてここから引用
// https://qiita.com/utibenkei/items/65b56c13f43ce5809561


#if UNITY_ANDROID && UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif

namespace CamraAndMicrophoneAuthorization
{
    // iOS/Androidプラットフォームで動作するランタイムパーミッション要求のサンプル
    // 参考URL: https://forum.unity.com/threads/requestuserauthorization-as-a-coroutine-bugged.380666/
    public class PermissionControllers : MonoBehaviour
    {
        IEnumerator Start ()
        {
            #if UNITY_IOS && UNITY_2018_1_OR_NEWER

            // iOS
            //
            // カメラパーミッションが許可されているか調べる
            if (!Application.HasUserAuthorization (UserAuthorization.WebCam)) {
                // 権限が無いので、カメラパーミッションのリクエストをする
                yield return RequestUserAuthorization (UserAuthorization.WebCam);
            }
            // マイクパーミッションが許可されているか調べる
            if (!Application.HasUserAuthorization (UserAuthorization.Microphone)) {
                // 権限が無いので、マイクパーミッションのリクエストをする
                yield return RequestUserAuthorization (UserAuthorization.Microphone);
            }
            // リクエストの結果、アプリ機能に必要なパーミッションが全て許可されたか調べる
            if (Application.HasUserAuthorization (UserAuthorization.WebCam) && Application.HasUserAuthorization (UserAuthorization.Microphone)) {
                // 権限が許可されたので、権限が必要なAPIを使用する処理へ進む（iOSでは権限拒否された状態でAPIを使用しようとするとアプリが落ちる）
                findWebCams ();
                findMicrophones ();
            } else {
                // 権限が許可されなかったので、ユーザーに対して権限の使用用途の説明を表示して自分でOSのアプリ設定画面で権限許可を行うようにアナウンスする必要がある。
                // (iOSでは初回の権限リクエストが拒否された場合は、次回からはリクエスト自体が表示されなくなる)
            }       

            #elif UNITY_ANDROID && UNITY_2018_3_OR_NEWER

            // Android
            //
            // カメラパーミッションが許可されているか調べる
            if (!Permission.HasUserAuthorizedPermission (Permission.Camera)) {
                // 権限が無いので、カメラパーミッションのリクエストをする
                yield return RequestUserPermission (Permission.Camera);
            }
            // マイクパーミッションが許可されているか調べる
            if (!Permission.HasUserAuthorizedPermission (Permission.Microphone)) {
                // 権限が無いので、マイクパーミッションのリクエストをする
                yield return RequestUserPermission (Permission.Microphone);
            }
            // リクエストの結果、アプリ機能に必要なパーミッションが全て許可されたか調べる
            if (Permission.HasUserAuthorizedPermission (Permission.Camera) && Permission.HasUserAuthorizedPermission (Permission.Microphone)) {
                // 権限が許可されたので、権限が必要なAPIを使用する処理へ進む
                findWebCams ();
                findMicrophones ();
            } else {
                // 権限が許可されなかったので、ユーザーに対して権限の使用用途の説明を表示してから再度のリクエストを行う。
                // もしも拒否時に「今後表示しない」がチェックされた場合は、次回からリクエスト自体が表示されなくなる、
                // そのためユーザーには自分でOSのアプリ設定画面で権限許可を行うようにアナウンスする必要がある。
                // （Permissionクラスにはそれらの違いを調べる方法は用意されていない）
            }

            #endif

            yield break;
        }

#if (UNITY_IOS && UNITY_2018_1_OR_NEWER) || (UNITY_ANDROID && UNITY_2018_3_OR_NEWER)
        bool isRequesting;

        // OSの権限要求ダイアログを閉じたあとに、アプリフォーカスが復帰するのを待ってから権限の有無を確認する必要がある
        IEnumerator OnApplicationFocus(bool hasFocus)
        {
            // iOSプラットフォームでは1フレーム待つ処理がないと意図通りに動かない。
            yield return null;

            if (isRequesting && hasFocus)
            {
                isRequesting = false;
            }
        }

        #if UNITY_IOS
        IEnumerator RequestUserAuthorization(UserAuthorization mode)
        {
            isRequesting = true;
            yield return Application.RequestUserAuthorization(mode);
            // iOSではすでに権限拒否状態だとダイアログが表示されず、フォーカスイベントが発生しない。
            // その状態を判別する方法が見つからないので、タイムアウト処理をする。

            // アプリフォーカスが戻るまで待機する
            float timeElapsed = 0;
            while (isRequesting)
            {
                if (timeElapsed > 0.5f){
                    isRequesting = false;
                    yield break;
                }
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            yield break;
        }
        #elif UNITY_ANDROID
        IEnumerator RequestUserPermission(string permission)
        {
            isRequesting = true;
            Permission.RequestUserPermission(permission);
            // Androidでは「今後表示しない」をチェックされた状態だとダイアログは表示されないが、フォーカスイベントは通常通り発生する模様。
            // したがってタイムアウト処理は本来必要ないが、万が一の保険のために一応やっとく。

            // アプリフォーカスが戻るまで待機する
            float timeElapsed = 0;
            while (isRequesting)
            {
                if (timeElapsed > 0.5f){
                    isRequesting = false;
                    yield break;
                }
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            yield break;
        }
        #endif
#endif

        void findWebCams ()
        {
            foreach (var device in WebCamTexture.devices) {
                Debug.Log ("Name: " + device.name);
            }
        }

        void findMicrophones ()
        {
            foreach (var device in Microphone.devices) {
                Debug.Log ("Name: " + device);
            }
        }
    }
}
