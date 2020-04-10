import web, urllib

# Imports for finding latest file
import glob
import os

import xml.etree.ElementTree as ET

from cheroot.server import HTTPServer
from cheroot.ssl.builtin import BuiltinSSLAdapter

HTTPServer.ssl_adapter = BuiltinSSLAdapter(
    certificate = './Certificates/jsco-cert.crt',
    private_key = './Certificates/jsco-cert.key')

urls = (
	'/index', 'index',
        '/meals', 'meals',
        '/staples', 'staples',
        '/mealdata', 'mealdata'
)

#Global dictionay of common items
t_globals = dict(datestr = web.datestr,)

# The rendering engine for html templates
render = web.template.render('/home/pi/templates', globals=t_globals)

# Add the engine to globals so it can reference itself
render._keywords['globals']['render'] = render

filedir = '/home/pi/data'

class staples:
    def GET(self):
        try:
            with open(filedir + '/static/staples.xml') as staplesfile:
                output = staplesfile.read()
        except:
            output = 'staples file is missing'
        return output      

    def POST(self):
        x = web.data()
        
        # Remove the header "data="
        cleaned = x[9:]
        
        # Write the XML string to file
        fout = open(filedir + '/static/staples.xml', 'w')

        # Decode url included characters
        fout.write(urllib.unquote_plus(cleaned))
        fout.close()
        return x

class mealdata:
    def GET(self):
        try:
            with open(filedir + '/static/mealdata.xml') as staplesfile:
                output = staplesfile.read()
        except:
            output = 'mealdata file is missing'
        return output

    def POST(self):
        x = web.data()

        # Remove the header "data="
        cleaned = x[9:]

        # Write the XML string to file
        fout = open(filedir + '/static/mealdata.xml', 'w')

        # Decode url included characters
        fout.write(urllib.unquote_plus(cleaned))
        fout.close()
        return x

class index:
    def GET(self):
        x = web.input(date="latest")
        filename = filedir

        if x.date == 'latest':
           list_of_files = glob.glob( filedir + '/*.xml' )
           latest_file = max(list_of_files, key=os.path.getctime)
           filename = latest_file 
        else:
           filename = filedir + '/' + x.date + '.xml'

        try:
            with open(filename, 'r') as myfile:
                output = myfile.read()
        except:
            output = filename + ' is missing'

        return output

    def POST(self):
       
        # This is the xml string passed from the client
        x = web.data()
        
	# Remove the header "data="
        cleaned = x[9:]
        date = x[:8]
   
        # Write the XML string to file
        #filedir = '/home/pi/data'
        fout = open(filedir + '/' + date +'.xml', 'w')

	# Decode url included characters
        fout.write(urllib.unquote_plus(cleaned))
        fout.close()
        return x

class meals:
    def GET(self):
        
        list_of_files = glob.glob( filedir + '/*.xml' )
        latest_file = max(list_of_files, key=os.path.getctime)

        tree = ET.parse(latest_file)
        root = tree.getroot()

        meals = []

        for subelem in root.findall('ArrayOfSelectedMeal/SelectedMeal'):
           # Do something with date here
           date = subelem.find('DateTime')
           meals.append(date.text)

           for mealelem in subelem.findall('Meals/string'):
              meals.append(mealelem.text)
        #meals = ['meal1', 'meal2', 'meal3']
        # Show page
        return render.base(render.listing(meals))

if __name__ == "__main__":
    app = web.application(urls, globals())
    app.run()
