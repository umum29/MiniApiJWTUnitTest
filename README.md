https://blog.darkthread.net/blog/testing-min-api/?fbclid=IwAR3MdGhtEq2XQ9jgdIw6hEyT58cWlkhIItM9D6eAGcypaY2pOMXor-MScnQ<br/>
<b>"public partial class Program { }"</b> in <b>MiniApi project</b> is for converting to a <i>public class</i> for testing!!!<br/>
<b>UnitTest project</b> needs to reference the original <b>MiniAPi project</b>, and also install package of "Microsoft.AspNetCore.Mvc.Testing" for creating new WebHost(Application) ---> "new WebApplicationFactory<Program>()"
