#include "plato_cpu.hpp"
#include <cassert>

struct Counted {
    static int copies;
    plato::String text;
    double padding[40]{};
    Counted() = default;
    Counted(const Counted& other) : text(other.text) {
        ++copies;
        for (int i = 0; i < 40; ++i) padding[i] = other.padding[i];
    }
    Counted& operator=(const Counted&) = default;
};
int Counted::copies = 0;

int main() {
    using namespace plato;
    Counted rows[1];
    rows[0].text = u"original heap-backed text that cannot use a short-string buffer";
    Array<Counted> xs{rows, 1};
    static_assert(std::is_same_v<decltype(array_at_readonly(xs, 0)), const Counted&>);
    static_assert(std::is_same_v<decltype(array_at_readonly(Array<int32_t>{}, 0)), int32_t>);
    Counted::copies = 0;
    const auto& borrowed = array_at_readonly(xs, 0);
    assert(&borrowed == &rows[0] && Counted::copies == 0);
    auto owned = array_at_readonly(xs, 0);
    assert(Counted::copies == 1);
    const auto old_contract = At(xs, 0);
    assert(Counted::copies == 2);
    rows[0].text[0] = u'x';
    assert(owned.text[0] == u'o' && old_contract.text[0] == u'o');
    return 0;
}
