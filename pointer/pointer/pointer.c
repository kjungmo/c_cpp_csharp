#include <stdio.h>

void main(void)
{
	int nData = 10; // 정수형 데이터 선언 및 초기화
	int *pnData = &nData; // 포인터 변수 선언, nData의 주소로 초기화

	printf("%d, %d\n", nData, *pnData); // 간접 지정 연산을 통한 값 출력
	printf("%p, %p\n", &nData, pnData); // 주소 출력

	*pnData = 20; // 
	printf("%d\n", nData);
}
