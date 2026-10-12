#include <stdio.h>
int reverse(int num){
    
    if (-2147483648<=num && num<=2147483647){
        int rev=0;
        int digit=0;
        while(num!=0){
            digit=num%10;
            rev=rev*10+digit;
           num=num/10;
        }
        return rev;
    }
 
  return 0;
}

int main(){
    int x=1234556;
    printf("Reverse of the number is %d ",reverse(x));
    return 0;
}