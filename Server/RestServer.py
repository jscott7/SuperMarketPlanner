import web, urllib

urls = (
	'/index', 'index'
)

class index:
    def GET(self):
        x = web.input()
        return "<b>text=<b>"

    def POST(self):
   
        x = web.data()
        
        cleaned = x[5:]
       
        print urllib.unquote_plus(cleaned)
      
        filedir = '/home/pi/Scratch/test'
        fout = open(filedir + '/test.xml', 'w')
        fout.write(urllib.unquote_plus(cleaned))
        fout.close()
        return x

#class server:
#    def GET(self):
#        raise web.seeother('/static/sample.xml')

if __name__ == "__main__":
    app = web.application(urls, globals())
    app.run()
