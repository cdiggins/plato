// TestAssemblyCode.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

/*
// https://www.pcjs.org/documents/books/mspl13/masm/mpguide/

Special Purpose registers:

    SP (Stack Pointer) - The SP register points to the current location within
    the stack segment. Pushing a value onto the stack decreases the value of SP.
    Popping from the stack increases the value of SP.

    IP (Instruction Pointer)  - The IP register always contains the address of
    the next instruction to be executed. You cannot directly access or change
    the instruction pointer. Instead use calls, jumps, loops, or interrupts.




Register operands

    mov     bx, 10          ; Load constant to BX
    add     ax, bx          ; Add AX and BX
    jmp     di              ; Jump to the address in DI

    
Pointer to memory. An offset stored in a base or index register is often used as a pointer into
memory. 

    mov     [bx], dl ; Store DL in indirect memory operand
    inc     bx       ; Increment register operand
    mov     [bx], dl ; Store DL in new indirect memory operand

Question: what are the base / index registers. 

Immediate operands 

Immediate operands are constant value that is specified at assembly time.
It can be a constant or the result of a constant expression. 

    mov     cx, 20          ; Load constant to register
    add     var, 1Fh        ; Add hex constant to variable
    sub     bx, 25 * 80     ; Subtract constant expression

Question where does the result of an add/subtract operation go? I think the first operand.


Loop-Generating Directives

.WHILE condition statements .ENDW

This loop copies one buffer to another until a `$' character
(marking the end of the string) is found:

    .DATA
        buf1    BYTE "This is a string",'$'
        buf2    BYTE 100 DUP (?)
    .CODE
        sub     bx, bx            ; Zero out bx
    .WHILE  (buf1[bx] != '$')
        mov     al, buf1[bx]      ; Get a character
        mov     buf2[bx], al      ; Move it to buffer 2
        inc     bx                ; Count forward
    .ENDW

The .BREAK and .CONTINUE directives can be used to terminate a .REPEAT or
.WHILE loop prematurely. These directives allow an optional .IF clause for
conditional breaks. The syntax is

    .BREAK «.IF condition»
    .CONTINUE «.IF condition»

The following loop accepts only the keys in the range `0' to `9' and
terminates when ENTER is pressed.

    .WHILE 1                ; Loop forever
        mov     ah, 08h         ; Get key without echo
        int     21h
        .BREAK 
            .IF al == 13     ; If ENTER, break out of the loop
        .CONTINUE 
            .IF (al < '0') || (al > '9') ; If not a digit, continue looping
        mov     dl, al          ; Save the character for processing
        mov     ah, 02h         ; Output the character
        int     21h
    .ENDW

    Translates to:

      .WHILE 1
    0017                    *@C0001:
    0017  B4 08                       mov    ah, 08h
    0019  CD 21                       int    21h
                                .BREAK .IF al == 13
    001B  3C 0D             *         cmp    al, 00Dh
    001D  74 10             *         je     @C0002
                                .CONTINUE .IF (al  '0') || (al  '9')
    001F  3C 30             *         cmp    al, '0'
    0021  72 F4             *         jb     @C0001
    0023  3C 39             *         cmp    al, '9'
    0025  77 F0             *         ja     @C0001
    0027  8A D0                       mov    dl, al
    0029  B4 02                       mov    ah, 02h
    002B  CD 21                       int    21h
                                .ENDW
    002D  EB E8             *         jmp    @C0001
    002F                    *@C0002:

    SUB and CMP set the same flags.

Internally, the CMP instruction is the same as the SUB instruction, except
that CMP does not change the destination operand. Both set flags according
to the result that the subtraction generates.

Table 7.1 lists conditional-jump instructions for each comparison
relationship and shows the flags that are tested to see if the relationship
is true. Note the difference in instructions depending on the sign of the
operands. Some of these are equivalent to instructions listed in the
previous section.

Table   7.1 Conditional-Jump Instructions Used after Compare Instruction

 Jump           Signed         Flags Tested   Unsigned       Flags Tested
 Condition      Compare        (Jump if True)  Compare        (Jump if True)

 =    (Equal)   JE             ZF = 1         JE             ZF = 1

(Not equal)    JNE            ZF = 0         JNE            ZF = 0

>    (Greater  JG or JNLE     ZF = 0 and     JA or JNBE     CF = 0 and
than)                         SF = 0F                       ZF = 0

<=   (Less     JLE or JNG     ZF = 1 or      JBE or JNA     CF = 1 or
than                          SF  0F                        ZF = 1
        or
equal to)

<    (Less     JL or JNGE     SF  0F         JB or JNAE     CF = 1
than)

>=  (Greater   JGE or JNL     SF = 0F        JAE or JNB     CF = 0
Jump           Signed         Flags Tested   Unsigned       Flags Tested
Condition      Compare        (Jump if True)  Compare        (Jump if True)
────────────────────────────────────────────────────────────────────────────
>=  (Greater   JGE or JNL     SF = 0F        JAE or JNB     CF = 0
than
        or
equal to)

────────────────────────────────────────────────────────────────────────────


In the CMP instruction, the mnemonic names always refer to the relationship
of the first operand to the second operand. For instance, in this example JG
tests whether the first operand is greater than the second.

    cmp     ax, bx  ; Compares ax and bx
    jg      contin  ; Equivalent to:  If ( ax > bx ) goto contin
    jl      next    ; Equivalent to:  If ( ax < bx ) goto next

 // https://learn.microsoft.com/en-us/cpp/assembler/inline/using-and-preserving-registers-in-inline-assembly?source=recommendations&view=msvc-170

 https://learn.microsoft.com/en-us/cpp/assembler/inline/jumping-to-labels-in-inline-assembly?view=msvc-170 

 Both assembly instructions and goto statements can jump to labels inside or outside the __asm block.
 As in MASM programs, the dollar symbol ($) serves as the current location counter. It is a label for the instruction currently being assembled. In __asm blocks, its main use is to make long conditional jumps:

C++

Copy
   jne $+5 ; next instruction is 5 bytes long
   jmp farlabel ; $+5
   .
   .
   .
farlabel:



https://en.wikibooks.org/wiki/X86_Assembly/MASM_Syntax

http://www.c-jump.com/CIS77/ASM/Instructions/lecture.html


*/

// InlineAssembler_Calling_C_Functions_in_Inline_Assembly.cpp
// processor: x86
#include <stdio.h>


/*
// Similar to printf( format, hello, world );
void PrintHelloWorld()
{
    char format[] = "%s %s\n";
    char hello[] = "Hello";
    char world[] = "world";

    __asm
    {
        mov  eax, offset world
        push eax
        mov  eax, offset hello
        push eax
        mov  eax, offset format
        mov  eax, offset format
        push eax
        call printf
        //clean up the stack so that main can exit cleanly
        //use the unused register ebx to do the cleanup
        pop  ebx
        pop  ebx
        pop  ebx
    }
}
*/
__declspec (naked) int Funky(int x)
{
    int a;

    // Prolog 

    __asm 
    {      
        push    ebp //  Base pointer
        mov     ebp, esp // Set stack frame pointer
        sub     esp, __LOCAL_SIZE // Allocate space for locals

        // push        <registers>        ; Save registers
        sub x, 2
    }

    /* Function body */

    // Epilog
    __asm 
    {          
        mov      esp, ebp   // Restore stack pointer?
        pop      ebp        // Restore the base pointer? 
        ret                 // WHAT?!
    }
}


__declspec(naked) int __fastcall  power(int i, int j) {
    // calculates i^j, assumes that j >= 0

    // prolog
    __asm {
        push ebp
        mov ebp, esp
        sub esp, __LOCAL_SIZE
        // store ECX and EDX into stack locations allocated for i and j
        mov i, ecx
        mov j, edx
    }

    {
        int k = 1;   // return value
        while (j-- > 0)
            k *= i;
        __asm {
            mov eax, k
        };
    }

    // epilog
    __asm {
        mov esp, ebp
        pop ebp
        ret
    }
}

// FastPower(ebx=exp, ecx=val) 
// => eax=val^exp, jump to edx
__declspec(naked) int __fastcall FastPower()
{
    __asm mov eax, 1  // initialize return value

    __asm
    {
    loop_start:
        cmp ebx, 0      // compare ebx to zero
        je loop_exit    // if it is then leave 
        sub ebx, 1      // subtract 1
        imul eax, ecx   // multiply return result by ecx (no overflow)
        jmp loop_start  // go to loop beginning
    
    loop_exit:
        jmp edx
    }
}

/*
int main()
{
    int k = 99;
    __asm mov ebx, 3
    __asm mov ecx, 4
    __asm mov edx, next_instruction
    __asm jmp FastPower
next_instruction:
    __asm mov k, eax

    std::cout << "Hello World!\n" << k;
    //GetKey();
}
*/

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
