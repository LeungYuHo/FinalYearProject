import pygame 
from settings import *
from tile import Tile
from player import Player
from debug import debug
from support import *
from random import choice, randint
from weapon import Weapon
from ui import UI
from enemy import Enemy
from particles import AnimationPlayer
from magic import MagicPlayer
from upgrade import Upgrade

class Level:
	def __init__(self):

		# get the display surface 
		self.display_surface = pygame.display.get_surface()
		self.game_paused = False

		# sprite group setup
		self.visible_sprites = YSortCameraGroup()
		self.obstacle_sprites = pygame.sprite.Group()

		# attack sprites
		self.current_attack = None
		self.attack_sprites = pygame.sprite.Group()
		self.attackable_sprites = pygame.sprite.Group()

		# sprite setup
		self.map_number = 0
		self.create_map(self.map_number)

		# user interface 
		self.ui = UI()
		self.upgrade = Upgrade(self.player)

		# particles
		self.animation_player = AnimationPlayer()
		self.magic_player = MagicPlayer(self.animation_player)

		# title screen
		self.isOnMenu = True
		self.title_screen = pygame.image.load("../graphics/title_screen.png").convert()
		self.font = pygame.font.Font(UI_FONT,UI_FONT_SIZE + 8)

		# death ui
		btn_x, btn_y = WIDTH/2, 400
		self.respawn_rect = pygame.Rect(btn_x - 150, btn_y - 40, 300, 80)
		self.mmenu_rect = pygame.Rect(btn_x - 150, btn_y + 60, 300, 80)

		self.respawn_text_surf = self.font.render("Respawn", False, TEXT_COLOR)
		self.respawn_text_rect = self.respawn_text_surf.get_rect(center = (btn_x,btn_y))
		self.mmenu_text_surf = self.font.render("Title Screen", False, TEXT_COLOR)
		self.mmenu_text_rect = self.mmenu_text_surf.get_rect(center = (btn_x,btn_y + 100))

		self.font = pygame.font.Font(UI_FONT,UI_FONT_SIZE + 50)
		self.dead_text_surf = self.font.render("You Died", False, (150, 20, 30))
		self.dead_text_rect = self.dead_text_surf.get_rect(center = (WIDTH /2, 250))

	def create_map(self, index):
		layouts = {
			'boundary': import_csv_layout(f'../map{index}/map_FloorBlocks.csv'),
			'grass': import_csv_layout(f'../map{index}/map_Grass.csv'),
			'object': import_csv_layout(f'../map{index}/map_Objects.csv'),
			'entities': import_csv_layout(f'../map{index}/map_Entities.csv')
		}
		graphics = {
			'grass': import_folder('../graphics/Grass'),
			'objects': import_folder('../graphics/objects')
		}

		for style,layout in layouts.items():
			for row_index,row in enumerate(layout):
				for col_index, col in enumerate(row):
					if col != '-1':
						x = col_index * TILESIZE
						y = row_index * TILESIZE
						if style == 'boundary':
							Tile((x,y),[self.obstacle_sprites],'invisible')
						if style == 'grass':
							random_grass_image = choice(graphics['grass'])
							Tile(
								(x,y),
								[self.visible_sprites,self.obstacle_sprites,self.attackable_sprites],
								'grass',
								random_grass_image)

						if style == 'object':
							surf = graphics['objects'][int(col)]
							Tile((x,y),[self.visible_sprites,self.obstacle_sprites],'object',surf)

						if style == 'entities':
							if col == '394':
								self.player = Player(
									(x,y),
									[self.visible_sprites],
									self.obstacle_sprites,
									self.create_attack,
									self.destroy_attack,
									self.create_magic)
							else:
								if col == '390': monster_name = 'bamboo'
								elif col == '391': monster_name = 'spirit'
								elif col == '392': monster_name ='raccoon'
								else: monster_name = 'squid'
								Enemy(
									monster_name,
									(x,y),
									[self.visible_sprites,self.attackable_sprites],
									self.obstacle_sprites,
									self.damage_player,
									self.trigger_death_particles,
									self.add_exp)

	def create_attack(self):
		
		self.current_attack = Weapon(self.player,[self.visible_sprites,self.attack_sprites])

	def create_magic(self,style,strength,cost):
		if style == 'heal':
			self.magic_player.heal(self.player,strength,cost,[self.visible_sprites])

		if style == 'flame':
			self.magic_player.flame(self.player,cost,[self.visible_sprites,self.attack_sprites])

	def destroy_attack(self):
		if self.current_attack:
			self.current_attack.kill()
		self.current_attack = None

	def player_attack_logic(self):
		if self.attack_sprites:
			for attack_sprite in self.attack_sprites:
				collision_sprites = pygame.sprite.spritecollide(attack_sprite,self.attackable_sprites,False)
				if collision_sprites:
					for target_sprite in collision_sprites:
						if target_sprite.sprite_type == 'grass':
							pos = target_sprite.rect.center
							offset = pygame.math.Vector2(0,75)
							for leaf in range(randint(3,6)):
								self.animation_player.create_grass_particles(pos - offset,[self.visible_sprites])
							target_sprite.kill()
						else:
							target_sprite.get_damage(self.player,attack_sprite.sprite_type)

	def damage_player(self,amount,attack_type):
		if self.player.vulnerable:
			self.player.health -= amount
			self.player.vulnerable = False
			self.player.hurt_time = pygame.time.get_ticks()
			self.animation_player.create_particles(attack_type,self.player.rect.center,[self.visible_sprites])

	def trigger_death_particles(self,pos,particle_type):

		self.animation_player.create_particles(particle_type,pos,self.visible_sprites)

	def add_exp(self,amount):

		self.player.exp += amount

	def toggle_menu(self):

		self.game_paused = not self.game_paused

	def check_monsters(self):
		length = 0

		for sprite in self.attackable_sprites:
			if sprite.sprite_type == 'enemy':
				length += 1

		return length

	def next_level(self):
		self.display_surface.fill((10, 0, 20))

		# check if it is the last level
		if self.map_number == 3:
			self.font = pygame.font.Font(UI_FONT, 50)
			text1 = self.font.render("Thanks for playing", False, "#ffffff")
			self.font = pygame.font.Font(UI_FONT, 20)
			text2 = self.font.render("THIS WAS THE LAST LEVEL", False, "#ffffff")

			self.display_surface.blit(text1, text1.get_rect(center = (WIDTH /2, 300)))
			self.display_surface.blit(text2, text2.get_rect(center = (WIDTH /2, 400)))
			return

		self.font = pygame.font.Font(UI_FONT, 50)
		text1 = self.font.render("Going to the next level", False, "#ffffff")
		self.font = pygame.font.Font(UI_FONT, 20)
		text2 = self.font.render("PRESS ANY KEY TO CONTINUE", False, "#ffffff")

		self.display_surface.blit(text1, text1.get_rect(center = (WIDTH /2, 300)))
		self.display_surface.blit(text2, text2.get_rect(center = (WIDTH /2, 400)))

		if pygame.mouse.get_pressed()[0]:
			for sprite in self.visible_sprites:
				sprite.kill()
				del sprite

			self.map_number += 1
			self.create_map(self.map_number)

	def run(self):
		if self.isOnMenu:
			# draw title screen
			self.display_surface.blit(self.title_screen, self.title_screen.get_rect(topleft = (0, 0)))

			if True in pygame.key.get_pressed():
				self.player.health = self.player.stats['health'] * 0.5
				self.player.energy = self.player.stats['energy'] * 0.8
				self.player.hitbox.center = self.player.starting_pos
				self.isOnMenu = False
		else:
			self.visible_sprites.custom_draw(self.player)
			self.ui.display(self.player)
			
			if self.player.health < 0:
				if pygame.mouse.get_pressed()[0]:
					if self.respawn_rect.collidepoint(pygame.mouse.get_pos()):
						self.player.health = self.player.stats['health'] * 0.5
						self.player.energy = self.player.stats['energy'] * 0.8
						self.player.hitbox.center = self.player.starting_pos
					elif self.mmenu_rect.collidepoint(pygame.mouse.get_pos()):
						self.isOnMenu = True

				# respawn
				pygame.draw.rect(self.display_surface, UI_BG_COLOR, self.respawn_rect, border_radius=10)
				self.display_surface.blit(self.respawn_text_surf,self.respawn_text_rect)

				pygame.draw.rect(self.display_surface, UI_BG_COLOR, self.mmenu_rect, border_radius=10)
				self.display_surface.blit(self.mmenu_text_surf,self.mmenu_text_rect)

				self.display_surface.blit(self.dead_text_surf,self.dead_text_rect)
			else:
				if self.game_paused:
					self.upgrade.display()
				else:
					if self.map_number == 0 and self.check_monsters() < 32:
						self.next_level()
					elif self.check_monsters() == 0:
						self.next_level()
					else:
						self.visible_sprites.update()
						self.visible_sprites.enemy_update(self.player)
						self.player_attack_logic()


class YSortCameraGroup(pygame.sprite.Group):
	def __init__(self):

		# general setup 
		super().__init__()
		self.display_surface = pygame.display.get_surface()
		self.half_width = self.display_surface.get_size()[0] // 2
		self.half_height = self.display_surface.get_size()[1] // 2
		self.offset = pygame.math.Vector2()

		# creating the floor
		self.floor_surf = pygame.image.load('../graphics/tilemap/ground.png').convert()
		self.floor_rect = self.floor_surf.get_rect(topleft = (0,0))

	def custom_draw(self,player):

		# getting the offset 
		self.offset.x = player.rect.centerx - self.half_width
		self.offset.y = player.rect.centery - self.half_height

		# drawing the floor
		floor_offset_pos = self.floor_rect.topleft - self.offset
		self.display_surface.blit(self.floor_surf,floor_offset_pos)

		# for sprite in self.sprites():
		for sprite in sorted(self.sprites(),key = lambda sprite: sprite.rect.centery):
			offset_pos = sprite.rect.topleft - self.offset
			self.display_surface.blit(sprite.image,offset_pos)

	def enemy_update(self,player):
		enemy_sprites = [sprite for sprite in self.sprites() if hasattr(sprite,'sprite_type') and sprite.sprite_type == 'enemy']
		for enemy in enemy_sprites:
			enemy.enemy_update(player)
