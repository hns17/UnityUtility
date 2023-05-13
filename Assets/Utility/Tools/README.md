# UnityTools

- Unity 개발할 때 자주 사용하는 기능 모음



# 설치

- PackageManager를 통해 아래 git 주소를 추가
  - url : https://github.com/hns17/UnityUtility.git?path=Assets/Utility/Tools



# 목록

## GraphicRaycastTargetToggle

- Graphics Component에 포함된 RaycastTarget 정보를 하이어라키의 토글을 통해 설정할 수 있다.

![image-20230513064332557](C:\Users\hns17\AppData\Roaming\Typora\typora-user-images\image-20230513064332557.png)



## MonoSingleTon

- Monobehaviour 싱글톤 클래스
- 아래와 같은 형태로 상속받아 사용

```C#
class ClassName : MonoSingleton<ClassName>{
	...
}
```

