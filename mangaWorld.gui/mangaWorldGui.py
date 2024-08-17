import dearpygui.dearpygui as dpg
import json
import os
import subprocess

# TODO implement a progress bar by counting the current downloaded number of pages in relation to the total length of the selected volumes or chapters

# https://janakiev.com/blog/python-shell-commands/#using-the-subprocess-module
# https://stackoverflow.com/questions/11495783/redirect-subprocess-stderr-to-stdout

# needed code
dpg.create_context()
dpg.create_viewport(title='manga downloader', width=1100, height=600)
dpg.set_viewport_vsync(True)

# custom code here ===================================

PATH = "../Data/info/"
DOWNLOADER_EXECUTABLE = "../mangaDownloader/bin/Debug/net8.0/mangaDownloader"       # TODO change executable with dotnet run (to ensure that the application gets rebuilt every time ?? slow method)
SCRAPER_EXECUTABLE = "../mangaScraper/bin/Debug/net8.0/mangaScraper"

selected_manga_name = []
selected_volumes = []
selected_single_chapters = []

def list_info_files():
    return os.listdir(PATH)

# checkbox callback - when a checkbox is clicked, add or remove the chapter to the list of chapters to download
def select_single_chapter(sender, app_data, chapter):
    if (app_data == True):
        selected_single_chapters.append(chapter)
    if (app_data == False):
        selected_single_chapters.remove(chapter)

# select volume callback - when the button is pressed, the volume is added to the list of volumes to download
def select_whole_volume(sender, app_data, volume):

    selected_volumes.append(volume["volume"])

    for chapter in volume["chapters"]:
        dpg.set_value(chapter['chapterNum'], True)

    dpg.set_item_label(sender, "de-select volume")
    dpg.set_item_callback(sender, deselect_whole_volume)

# deselect volume callback - when the button is pressed, the volume is removed from the list of volumes to download
def deselect_whole_volume(sender, app_data, volume):

    selected_volumes.remove(volume["volume"])

    for chapter in volume["chapters"]:
        dpg.set_value(chapter['chapterNum'], False)

    dpg.set_item_label(sender, "select volume")
    dpg.set_item_callback(sender, select_whole_volume)

# debug function to print the list of selected volumes and chapters
def print_selected_items():
    print(f"entire volumes:\n{selected_volumes}\nsingle chapters:\n{selected_single_chapters}")
    
    generate_download_command()

# generates the dowload command that will be later executed
def generate_download_command():
    command = []
    command.append(DOWNLOADER_EXECUTABLE)
    command.append('-f')
    command.append(selected_manga_name[0])

    if len(selected_volumes) != 0:
        command.append('-v')
        for vol in selected_volumes:
            command.append(vol)

    if len(selected_single_chapters) != 0:
        command.append('-c')
        for chapt in selected_single_chapters:
            command.append(chapt)

    print(command)

    return command

# function executed when a manga is selected from the list of already scraped mangas (combo box)
def manga_selector_callback(senser, selected_manga):
    # load json file here
    selected_manga_name.clear()
    selected_manga_name.append(selected_manga)
    file = open(f"{ PATH }{ selected_manga }")
    data = json.load(file)
    file.close()

    # delete the window and all of it's childrens (to re-create it with new informations; all tags are deleted as well, so re-using old tag values is not a problem)
    dpg.delete_item("manga-volumes-list")
    selected_volumes.clear()
    selected_single_chapters.clear()
    
    # create a window to display the list of volumes and chapters of a manga
    manga_items_selector = dpg.add_window(tag = "manga-volumes-list", label = "manga volumes list", width = 300, height = 580, pos = (10 + 250 + 20, 10))

    for volume in data["volumes"]:

        # for each volume create a collapsable list of chapters
        volume_element = dpg.add_collapsing_header(tag = f"{volume['volume']}", label = f"{volume['volume']}", parent = manga_items_selector)
        
        dpg.add_button(label = "select volume", parent = volume_element, user_data = volume, callback = select_whole_volume)
        
        # for each chapter create a checkbox
        for chapt in volume["chapters"]:
            dpg.add_checkbox(tag = f"{chapt['chapterNum']}", label = f'{ chapt["chapterNum"]} - { chapt["numPages"] } pages', parent = volume_element, callback = select_single_chapter, user_data = f"{ volume['volume'] }:{ chapt['chapterNum'] }")

    # create a window with some buttons to execute functions
    functions = dpg.add_window(label = "functions", width = 200, pos = (10, 10 + 150))
    dpg.add_button(parent = functions, label = "download selected items", callback = execute_download_command)
    dpg.add_button(parent = functions, label = "debug info", callback = print_selected_items)

# execute a shell command passed as a parameter and prints out the results in a new output window
def execute_command(sender, app_data, user_data):
    
    # delete the output window (if already present)
    dpg.delete_item("output-window")

    # create the output window
    output_window = dpg.add_window(tag = "output-window", label = "output", width = 490, height = 300, pos = (10 + 250 + 20 + 300 + 20, 10))

    process = subprocess.Popen(user_data,
        stdout = subprocess.PIPE,
        stderr = subprocess.STDOUT,     # stderr redirected to stdout to be able to print it in the console (I had a strange bug where I couldn't read both stderr and stdout at the same time by using two distinct PIPEs)
        universal_newlines = True       # format output to UTF-8 and remove the '\n' and other characters
    )
    
    while True:
        output = process.stdout.readline().strip()      # read stdout line by line
        
        if len(output) != 0:
            print(output)
            dpg.add_text(output, parent = "output-window")
            scroll = dpg.get_y_scroll_max("output-window")
            dpg.set_y_scroll("output-window", scroll)       # automatically scrolls to the last line added

        return_code = process.poll()        # ask to the program if the execution has stopped or not
        if return_code is not None:
            end_string = f'finished with return code {return_code}'
            print(end_string)
            # adds the end string to the output window and scrolls down to show it (to be sure that the command has succesfully stopped or not)
            dpg.add_text(end_string, parent = "output-window")
            scroll = dpg.get_y_scroll_max("output-window")
            dpg.set_y_scroll("output-window", scroll + 20)
            break

# generates the scrape command and then executes it
def execute_download_command(sender, app_data, user_data):
    command = generate_download_command()
    execute_command(sender, app_data, user_data = command)

# generates the download command and then executes it
def execute_scrape_command(sender, app_data, user_data):
    link = dpg.get_value("manga-link")
    command = [SCRAPER_EXECUTABLE, link]
    execute_command(sender, app_data, user_data = command)

# shows the windows related to the manga downloader tool
def start_manga_downloader():
    dpg.delete_item(tool_selector)

    # window with a combo box to select the manga
    with dpg.window(tag = "manga-selector", label = "manga selector", width = 250, height = 100, pos = (10, 10)) as manga_selector:
        manga_combo = dpg.add_combo(tag = "manga-combo", items = list_info_files(), width = 240, callback = manga_selector_callback, default_value = "select a manga")

# shows the windows related to the manga scraper tool
def start_manga_scraper():
    dpg.delete_item(tool_selector)

    with dpg.window(tag = "scraper", label = "scraper", pos = (10, 10), width = dpg.get_viewport_width() - 520):
        dpg.add_text("paste the url of the manga to scape below")
        dpg.add_input_text(tag = "manga-link", hint = "ex. https://www.mangaworld.bz/manga/2278/berserk")
        dpg.add_button(label = "start scraping", callback = execute_scrape_command)

# TODO make it a little better looking
# shows a simple window with two buttons to select which tool to start
with dpg.window(label = "select tool", pos = (dpg.get_viewport_width() / 2 - 20, dpg.get_viewport_height() / 2 - 20)) as tool_selector:
    dpg.add_button(label = "manga scraper", callback = start_manga_scraper)
    dpg.add_button(label = "manga downloader", callback = start_manga_downloader)
    
# ====================================================

# needed code
dpg.setup_dearpygui()
dpg.show_viewport()
dpg.start_dearpygui()
dpg.destroy_context()