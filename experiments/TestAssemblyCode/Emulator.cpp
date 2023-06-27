#include <vector>
#include <cassert>
#include <iostream>
#include <iostream>

// 18 instructions. 
// Instructions could be combined with data. 
// 6 bits (64 instruction) + 10 bits (1024) 

enum OpCode
{
    // Stack operations 
    push,
    swap,
    pop,
    dup,
    
    // Math operations
    add_int,
    sub_int,
    mul_int,

    // Control flow operations 
    swap_if,
    while_loop,

    // Function operations 
    apply,
    compose,
    quote,
    ret,

    // List operations
    new_list,
    count,
    push_list,
    pop_list,
    swap_into,

    // reference count operations 
    inc_ref,
    dec_ref,
};

struct List  
{
    int32_t refcount; 
    std::vector<void*>* data;

    List()
    {
        data = new std::vector<void*>();
    }
    
    template<typename T>
    void Add(T x)
    {
        data->push_back((void*)x);
    }

    void* Pop()
    {
        auto r = data->back();
        data->pop_back();
        return r;
    }

    void** StartData()
    {
        return &((*data)[0]);
    }

    void* LastItem()
    {
        return (data->back());
    }

    void RemoveReturn()
    {
        assert(IsFunction());
        data->pop_back();
    }

    bool IsFunction()
    {
        return (data->size() > 0 && data->back() == (void*)ret);
    }
};

// b => compose apply 
// dip => swap quote b
// dip2 => [dip] dip
// ifte => [apply] dip2 rotl swapif
// rotl => swapd swap
// rotr => swap swapd
// swapd => [swap] dip
// linrec = [if] [then] [else1] [else2] 
// binrec = [if] [then] [else1] [else2] // else1 => two parts 
// small => count 1 lteq 

/*
 DEFINE qsort ==
   [small]
   []
   [uncons [>] split]
   [enconcat]
   binrec.
*/

// and or not eq neq not
// lt lteq gt gteq 
// while => dup 
// 

// applyd => [apply] dip

// b [x] applyif => [pop x] [pop] swapif apply
// [cond] [body] while => [[x] while] [x] applyd applyif
// => cond 
// ifte => [x] [[x] while] applyd => x [[x] while] applyif

struct CodeBuilder
{
    template<typename T>
    CodeBuilder& Push(T v) { AddOp(push); list.Add(v); return *this; }
    CodeBuilder& Swap() { return AddOp(swap); }
    CodeBuilder& Pop() { return AddOp(pop); }
    CodeBuilder& Dup() { return AddOp(dup); }

    CodeBuilder& AddInt() { return AddOp(add_int); }
    CodeBuilder& SubInt() { return AddOp(sub_int); }
    CodeBuilder& MulInt() { return AddOp(mul_int); }

    CodeBuilder& SwapIf() { return AddOp(swap_if); }

    CodeBuilder& Apply() { return AddOp(apply); }
    CodeBuilder& Compose() { return AddOp(compose); }
    CodeBuilder& Ret() { return AddOp(ret); }
    CodeBuilder& Quote() { return AddOp(quote); }

    CodeBuilder& NewList() { return AddOp(new_list); }
    CodeBuilder& Count() { return AddOp(count); }
    CodeBuilder& SwapInto() { return AddOp(swap_into); }
    CodeBuilder& PushList() { return AddOp(push_list); }
    CodeBuilder& PopList() { return AddOp(pop_list); }

    CodeBuilder& IncRef() { return AddOp(inc_ref); }
    CodeBuilder& DecRef() { return AddOp(dec_ref); }

    CodeBuilder& While() { return AddOp(while_loop); }

    CodeBuilder& AddOp(OpCode code)
    {
        list.Add(code);
        return *this;
    }

    CodeBuilder& Quotation(CodeBuilder& cb)
    {
        list.Add(&cb.list);
        return *this;
    }

    List list; 
};

struct Emulator 
{
    std::vector<void*> stack;
    std::vector<List*> call_stack;
    std::vector<int32_t*> return_stack;

    void Execute(CodeBuilder& cb)
    {
        Execute(cb.list);
    }

    void Execute(List& list)
    {
        ip = (int32_t*)list.StartData();
        while (*ip != ret)
        {
            execute_next();
        }
    }

    int32_t* ip;

    List* top_list() { return (List*)top(); }

    void*& top() { return stack[stack.size() - 1]; }
    void*& second() { return stack[stack.size() - 2]; }

    template<typename T>
    void push_value(T any) { stack.push_back((void*)any); }

    template<typename T>
    T pop_value() {
        T r = (T)top();
        stack.pop_back();
        return r;
    }

    void execute_next() 
    {
        switch ((OpCode)*ip++)
        {
        case push:
            push_value(*ip++);
            break;

        case swap:
            std::swap(top(), second());
            break;
        
        case pop:
            stack.pop_back();
            break;

        case dup:
            stack.push_back(top());
            break;

        case quote:
        {
            auto xs = new List();
            xs->Add(push);
            xs->Add(stack.back());
            xs->Add(ret);
            assert(xs->data->size() == 3);
            stack.back() = xs;
            break;
        }

        case apply:
        {
            List* f = pop_value<List*>();
            call_stack.push_back(f);
            return_stack.push_back(ip);
            ip = (int32_t*)f->StartData();
            break;
        }

        case ret:
        {
            List* current = call_stack.back();
            call_stack.pop_back();
            ip = return_stack.back();
            return_stack.pop_back();
            delete current;
            break;
        }

        case compose:
        {
            List* b = pop_value<List*>();
            List* a = top_list();
            assert(b->refcount == 0);
            assert(a->refcount == 0);
            a->RemoveReturn();
            a->Add(push);
            a->Add(b);
            a->Add(apply);
            a->Add(ret);
            break;
        }
       
        case add_int:
            push_value(pop_value<int>() + pop_value<int>());
            break;

        case sub_int:
            push_value(pop_value<int>() - pop_value<int>());
            break;
        
        case mul_int:
            push_value(pop_value<int>() * pop_value<int>());
            break;

        case swap_if:
        {
            auto n = pop_value<int32_t>();
            if (n != 0)
            {
                std::swap(top(), second());
            }
            break;
        }

        case new_list:
            push_value(new List());
            break;

        case push_list:
            top_list()->Add(pop_value<void*>());
            break;

        case pop_list:
            push_value(top_list()->Pop());
            break;

        case count:
            push_value(top_list()->data->size());
            break;

        case swap_into:
        {
            auto i = pop_value<int>();
            std::swap((*top_list()->data)[i], top());
            break;
        }
   
        case inc_ref:
            top_list()->refcount++;

        case dec_ref:
            if (top_list()->refcount-- == 0)
                delete top_list();

        default:
            break;
        }
    }
};

void output_stack(Emulator& emu)
{
    std::cout << "Stack has " << emu.stack.size() << " elements" << std::endl;
    for (auto x = emu.stack.rbegin(); x != emu.stack.rend(); x++)
    {
        std::cout << *x << std::endl;
    }
}

int main()
{
    CodeBuilder cb;
    Emulator emu;

    //cb.Push(1).Push(2).AddInt().Ret();    
    //emu.Execute(cb);
   
    cb.Push(1).Push(2).Dup().Quote().Swap().Pop().Swap().Pop().Apply(); 

    //cb.NewList().Push(42).Append().Quote().Apply().Push(99).Push(0).SwapInto();
    emu.Execute(cb);

    output_stack(emu);
}

